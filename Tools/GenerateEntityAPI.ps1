# Regenerates EntityAPI.cs — one IEntityComponent class per block, matching Unity EntityAPIGenerator logic.
param(
    [string]$ProjectRoot = (Split-Path $PSScriptRoot -Parent),
    [string]$RuntimeRoot = "Assets/_Project/Develop/Runtime"
)

$runtimePath = Join-Path $ProjectRoot $RuntimeRoot
$outputPath = Join-Path $runtimePath "Gameplay/EntitiesCore/Generated/EntityAPI.cs"

$knownFullTypes = @{
    "Entity" = "Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity"
    "bool" = "System.Boolean"
    "int" = "System.Int32"
    "float" = "System.Single"
    "double" = "System.Double"
    "string" = "System.String"
    "Rigidbody" = "UnityEngine.Rigidbody"
    "Transform" = "UnityEngine.Transform"
    "Animator" = "UnityEngine.Animator"
    "Vector2" = "UnityEngine.Vector2"
    "Vector3" = "UnityEngine.Vector3"
}

function Get-ValidTypeName([string]$typeName) {
    if ($typeName -match '`') {
        $base = $typeName.Substring(0, $typeName.IndexOf('`'))
        $argsPart = [regex]::Match($typeName, '<(.+)>').Groups[1].Value
        $args = $argsPart -split ',\s*' | ForEach-Object { Get-ValidTypeName $_.Trim() }
        return "$base<$(($args -join ', '))>"
    }
    return $typeName
}

function Remove-SuffixIfExists([string]$str, [string]$suffix) {
    if ($str.EndsWith($suffix)) { return $str.Substring(0, $str.Length - $suffix.Length) }
    return $str
}

function Get-VariableNameFrom([string]$name) {
    return [char]::ToLowerInvariant($name[0]) + $name.Substring(1)
}

function Resolve-TypeName([string]$rawType, [string[]]$usings, [string]$fileNamespace, [hashtable]$typeRegistry) {
    $rawType = $rawType.Trim()
    if ($rawType -match '^(.+)<(.+)>$') {
        $base = Resolve-TypeName $Matches[1] $usings $fileNamespace $typeRegistry
        $args = $Matches[2] -split ',\s*' | ForEach-Object { Resolve-TypeName $_.Trim() $usings $fileNamespace $typeRegistry }
        return "$base<$(($args -join ', '))>"
    }

    if ($knownFullTypes.ContainsKey($rawType)) {
        return $knownFullTypes[$rawType]
    }

    if ($typeRegistry.ContainsKey($rawType)) {
        return $typeRegistry[$rawType]
    }

    foreach ($usingNs in $usings) {
        $candidate = "$usingNs.$rawType"
        if ($typeRegistry.ContainsValue($candidate)) {
            return $candidate
        }
    }

    $fromFileNs = "$fileNamespace.$rawType"
    if ($typeRegistry.ContainsValue($fromFileNs)) {
        return $fromFileNs
    }

    foreach ($usingNs in $usings) {
        return "$usingNs.$rawType"
    }

    return "$fileNamespace.$rawType"
}

function Get-Usings([string]$content) {
    $usings = @()
    foreach ($match in [regex]::Matches($content, 'using\s+([\w\.=]+)\s*;')) {
        $value = $match.Groups[1].Value
        if ($value -notmatch '=') {
            $usings += $value
        }
    }
    return $usings
}

function Get-IEntityComponentClasses([string]$content) {
    $results = @()
    $pattern = 'public\s+(?:sealed\s+)?class\s+(\w+)\s*:\s*[^\{]*IEntityComponent'
    $matches = [regex]::Matches($content, $pattern)

    foreach ($match in $matches) {
        $className = $match.Groups[1].Value
        $startBrace = $content.IndexOf('{', $match.Index + $match.Length)
        if ($startBrace -lt 0) { continue }

        $depth = 0
        $endBrace = -1
        for ($index = $startBrace; $index -lt $content.Length; $index++) {
            $char = $content[$index]
            if ($char -eq '{') { $depth++ }
            elseif ($char -eq '}') {
                $depth--
                if ($depth -eq 0) {
                    $endBrace = $index
                    break
                }
            }
        }

        if ($endBrace -lt 0) { continue }

        $classBody = $content.Substring($startBrace + 1, $endBrace - $startBrace - 1)
        $results += @{ Name = $className; Body = $classBody }
    }

    return $results
}

function Test-HasEmptyConstructor([string]$fullTypeName) {
    if ($fullTypeName -match '^UnityEngine\.') { return $false }
    if ($fullTypeName -match '^System\.') { return $true }
    return $true
}

# Build type registry from all files first
$typeRegistry = @{}
$csFiles = Get-ChildItem -Path $runtimePath -Filter "*.cs" -Recurse |
    Where-Object { $_.FullName -notmatch "Generated|Editor|Entity\.cs|IEntityComponent\.cs" }

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $nsMatch = [regex]::Match($content, 'namespace\s+([\w\.]+)')
    if (-not $nsMatch.Success) { continue }
    $namespace = $nsMatch.Groups[1].Value

    foreach ($classMatch in [regex]::Matches($content, 'public\s+(?:sealed\s+)?(?:class|enum|struct)\s+(\w+)')) {
        $typeRegistry[$classMatch.Groups[1].Value] = "$namespace.$($classMatch.Groups[1].Value)"
    }
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore")
[void]$sb.AppendLine("{")
[void]$sb.AppendLine("`tpublic partial class Entity")
[void]$sb.AppendLine("`t{")

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    if ($content -notmatch 'IEntityComponent') { continue }

    $nsMatch = [regex]::Match($content, 'namespace\s+([\w\.]+)')
    if (-not $nsMatch.Success) { continue }
    $namespace = $nsMatch.Groups[1].Value
    $usings = Get-Usings $content
    $classes = Get-IEntityComponentClasses $content

    foreach ($classInfo in $classes) {
        $typeName = $classInfo.Name
        $classBody = $classInfo.Body
        $fullTypeName = "$namespace.$typeName"
        $componentName = Remove-SuffixIfExists $typeName "Component"
        $modifiedName = $componentName + "C"

        [void]$sb.AppendLine("`t`tpublic $fullTypeName $modifiedName => GetComponent<$fullTypeName>();")
        [void]$sb.AppendLine("")

        $fieldMatches = [regex]::Matches($classBody, 'public\s+([\w\.<>,\[\]]+)\s+(\w+)\s*;')
        $fields = @()
        foreach ($fieldMatch in $fieldMatches) {
            $resolvedType = Resolve-TypeName $fieldMatch.Groups[1].Value $usings $namespace $typeRegistry
            $fields += @{ Type = (Get-ValidTypeName $resolvedType); Name = $fieldMatch.Groups[2].Value }
        }

        if ($fields.Count -eq 1 -and $fields[0].Name -eq 'Value') {
            $fieldType = $fields[0].Type
            $varName = Get-VariableNameFrom $fields[0].Name

            [void]$sb.AppendLine("`t`tpublic $fieldType $componentName => $modifiedName.$($fields[0].Name);")
            [void]$sb.AppendLine("")

            [void]$sb.AppendLine("`t`tpublic bool TryGet$componentName(out $fieldType $varName)")
            [void]$sb.AppendLine("`t`t{")
            [void]$sb.AppendLine("`t`t`tbool result = TryGetComponent(out $fullTypeName component);")
            [void]$sb.AppendLine("`t`t`tif (result == true)")
            [void]$sb.AppendLine("`t`t`t`t$varName = component.$($fields[0].Name);")
            [void]$sb.AppendLine("`t`t`telse")
            [void]$sb.AppendLine("`t`t`t`t$varName = default($fieldType);")
            [void]$sb.AppendLine("`t`t`treturn result;")
            [void]$sb.AppendLine("`t`t}")
            [void]$sb.AppendLine("")

            if (Test-HasEmptyConstructor $fieldType) {
                [void]$sb.AppendLine("`t`tpublic Entity Add$componentName()")
                [void]$sb.AppendLine("`t`t{")
                [void]$sb.AppendLine("`t`t`treturn AddComponent(new $fullTypeName() { $($fields[0].Name) = new $fieldType() }); ")
                [void]$sb.AppendLine("`t`t}")
                [void]$sb.AppendLine("")
            }
        }

        if ($fields.Count -gt 0) {
            $paramNames = @{}
            $params = ($fields | ForEach-Object {
                $varName = Get-VariableNameFrom $_.Name
                $suffix = 2
                $uniqueName = $varName
                while ($paramNames.ContainsKey($uniqueName)) {
                    $uniqueName = "$varName$suffix"
                    $suffix++
                }
                $paramNames[$uniqueName] = $true
                "$($_.Type) $uniqueName"
            }) -join ", "

            $initParts = @()
            for ($fieldIndex = 0; $fieldIndex -lt $fields.Count; $fieldIndex++) {
                $field = $fields[$fieldIndex]
                $varName = Get-VariableNameFrom $field.Name
                $suffix = 2
                $uniqueName = $varName
                while ($initParts -contains "$($field.Name) = $uniqueName" -and $suffix -lt 100) {
                    $uniqueName = "$varName$suffix"
                    $suffix++
                }
                if ($fieldIndex -gt 0) {
                    $checkName = Get-VariableNameFrom $field.Name
                    $uniqueName = $checkName
                    $suffix = 2
                    $usedNames = @()
                    for ($priorIndex = 0; $priorIndex -lt $fieldIndex; $priorIndex++) {
                        $usedNames += Get-VariableNameFrom $fields[$priorIndex].Name
                    }
                    while ($usedNames -contains $uniqueName) {
                        $uniqueName = "$checkName$suffix"
                        $suffix++
                    }
                }
                $initParts += "$($field.Name) = $uniqueName"
            }

            # Rebuild params/init with consistent unique names
            $paramNamesMap = @{}
            $paramsList = @()
            $initList = @()
            foreach ($field in $fields) {
                $baseName = Get-VariableNameFrom $field.Name
                $uniqueName = $baseName
                $suffix = 2
                while ($paramNamesMap.ContainsKey($uniqueName)) {
                    $uniqueName = "$baseName$suffix"
                    $suffix++
                }
                $paramNamesMap[$uniqueName] = $field.Name
                $paramsList += "$($field.Type) $uniqueName"
                $initList += "$($field.Name) = $uniqueName"
            }

            [void]$sb.AppendLine("`t`tpublic Entity Add$componentName($($paramsList -join ', '))")
            [void]$sb.AppendLine("`t`t{")
            [void]$sb.AppendLine("`t`t`treturn AddComponent(new $fullTypeName() { $($initList -join ', ') }); ")
            [void]$sb.AppendLine("`t`t}")
        }
        else {
            [void]$sb.AppendLine("`t`tpublic Entity Add$componentName()")
            [void]$sb.AppendLine("`t`t{")
            [void]$sb.AppendLine("`t`t`treturn AddComponent(new $fullTypeName()); ")
            [void]$sb.AppendLine("`t`t}")
        }
        [void]$sb.AppendLine("")
    }
}

[void]$sb.AppendLine("`t}")
[void]$sb.AppendLine("}")

[System.IO.File]::WriteAllText($outputPath, $sb.ToString())
Write-Host "Generated EntityAPI at $outputPath ($((Get-Item $outputPath).Length) bytes)"
