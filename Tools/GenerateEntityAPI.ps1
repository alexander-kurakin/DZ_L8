param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$script:PrimitiveTypeMap = @{
    'int'    = 'System.Int32'
    'float'  = 'System.Single'
    'bool'   = 'System.Boolean'
    'string' = 'System.String'
}

$script:GlobalTypeMap = @{
    'Vector3'              = 'UnityEngine.Vector3'
    'Transform'            = 'UnityEngine.Transform'
    'Collider'             = 'UnityEngine.Collider'
    'CapsuleCollider'      = 'UnityEngine.CapsuleCollider'
    'SphereCollider'       = 'UnityEngine.SphereCollider'
    'LayerMask'            = 'UnityEngine.LayerMask'
    'Animator'             = 'UnityEngine.Animator'
    'Rigidbody'            = 'UnityEngine.Rigidbody'
    'Entity'               = 'Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity'
    'Teams'                = 'Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature.Teams'
    'TakeDamageInfo'       = 'Assets._Project.Develop.Runtime.Gameplay.Features.TakeDamage.TakeDamageInfo'
    'ICompositeCondition'  = 'Assets._Project.Develop.Runtime.Utilities.Conditions.ICompositeCondition'
    'IEntityComponent'     = 'Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.IEntityComponent'
    'IEntitySystem'        = 'Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems.IEntitySystem'
    'Buffer'               = 'Assets._Project.Develop.Runtime.Utilities.Buffer'
    'ReactiveVariable'     = 'Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable'
    'ReactiveEvent'        = 'Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveEvent'
    'List'                 = 'System.Collections.Generic.List'
}

function Build-TypeRegistry([string]$rootPath) {
    $registry = @{}
    $files = Get-ChildItem -Path $rootPath -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "Generated" }

    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw
        $namespaceMatch = [regex]::Match($content, 'namespace\s+([\w\.]+)')
        if ($namespaceMatch.Success -eq $false) { continue }

        $namespace = $namespaceMatch.Groups[1].Value
        $typeMatches = [regex]::Matches($content, 'public\s+(?:partial\s+)?(?:class|enum|struct)\s+(\w+)')

        foreach ($typeMatch in $typeMatches) {
            $typeName = $typeMatch.Groups[1].Value
            $fullName = "$namespace.$typeName"

            if ($registry.ContainsKey($typeName) -eq $false) {
                $registry[$typeName] = $fullName
            }
        }
    }

    return $registry
}

function Get-FileUsings([string]$content) {
    $usings = @{}

    $usingMatches = [regex]::Matches($content, 'using\s+([\w\.]+)\s*;')
    foreach ($usingMatch in $usingMatches) {
        $usingNamespace = $usingMatch.Groups[1].Value
        $shortName = $usingNamespace.Split('.')[-1]
        $usings[$shortName] = $usingNamespace
    }

    $aliasMatches = [regex]::Matches($content, 'using\s+(\w+)\s*=\s*([\w\.]+)\s*;')
    foreach ($aliasMatch in $aliasMatches) {
        $usings[$aliasMatch.Groups[1].Value] = $aliasMatch.Groups[2].Value
    }

    return $usings
}

function Resolve-TypeName([string]$typeName, [string]$namespace, [hashtable]$usings, [hashtable]$registry) {
    $trimmedTypeName = $typeName.Trim()

    if ($trimmedTypeName.Contains('<')) {
        $genericMatch = [regex]::Match($trimmedTypeName, '^([^<]+)<(.+)>$')
        if ($genericMatch.Success -eq $false) {
            return $trimmedTypeName
        }

        $genericTypeName = $genericMatch.Groups[1].Value.Trim()
        $genericArgumentsRaw = $genericMatch.Groups[2].Value
        $genericArguments = Split-GenericArguments $genericArgumentsRaw
        $resolvedGenericTypeName = Resolve-TypeName $genericTypeName $namespace $usings $registry
        $resolvedArguments = @()

        foreach ($genericArgument in $genericArguments) {
            $resolvedArguments += (Resolve-TypeName $genericArgument $namespace $usings $registry)
        }

        return "$resolvedGenericTypeName<$($resolvedArguments -join ', ')>"
    }

    if ($script:PrimitiveTypeMap.ContainsKey($trimmedTypeName)) {
        return $script:PrimitiveTypeMap[$trimmedTypeName]
    }

    if ($script:GlobalTypeMap.ContainsKey($trimmedTypeName)) {
        return $script:GlobalTypeMap[$trimmedTypeName]
    }

    if ($trimmedTypeName.Contains('.')) {
        return $trimmedTypeName
    }

    if ($usings.ContainsKey($trimmedTypeName)) {
        return "$($usings[$trimmedTypeName]).$trimmedTypeName"
    }

    if ($registry.ContainsKey($trimmedTypeName)) {
        return $registry[$trimmedTypeName]
    }

    return "$namespace.$trimmedTypeName"
}

function Split-GenericArguments([string]$argumentsRaw) {
    $arguments = New-Object System.Collections.Generic.List[string]
    $currentArgument = New-Object System.Text.StringBuilder
    $depth = 0

    for ($charIndex = 0; $charIndex -lt $argumentsRaw.Length; $charIndex++) {
        $currentChar = $argumentsRaw[$charIndex]

        if ($currentChar -eq '<') {
            $depth++
            [void]$currentArgument.Append($currentChar)
            continue
        }

        if ($currentChar -eq '>') {
            $depth--
            [void]$currentArgument.Append($currentChar)
            continue
        }

        if ($currentChar -eq ',' -and $depth -eq 0) {
            $arguments.Add($currentArgument.ToString().Trim()) | Out-Null
            $currentArgument.Clear() | Out-Null
            continue
        }

        [void]$currentArgument.Append($currentChar)
    }

    if ($currentArgument.Length -gt 0) {
        $arguments.Add($currentArgument.ToString().Trim()) | Out-Null
    }

    return $arguments.ToArray()
}

function Get-VariableName([string]$name) {
    return $name.Substring(0, 1).ToLower() + $name.Substring(1)
}

function Remove-SuffixIfExists([string]$value, [string]$suffix) {
    if ($value.EndsWith($suffix)) {
        return $value.Substring(0, $value.Length - $suffix.Length)
    }

    return $value
}

$componentsRoot = Join-Path $ProjectRoot "Assets\_Project\Develop\Runtime"
$typeRegistry = Build-TypeRegistry $componentsRoot
$files = Get-ChildItem -Path $componentsRoot -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "Generated" }
$componentTypes = New-Object System.Collections.Generic.List[object]

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -notmatch 'IEntityComponent') { continue }

    $namespaceMatch = [regex]::Match($content, 'namespace\s+([\w\.]+)')
    if ($namespaceMatch.Success -eq $false) { continue }

    $namespace = $namespaceMatch.Groups[1].Value
    $usings = Get-FileUsings $content
    $classMatches = [regex]::Matches($content, 'public class (\w+)\s*:\s*IEntityComponent')

    foreach ($classMatch in $classMatches) {
        $className = $classMatch.Groups[1].Value
        $classStart = $classMatch.Index
        $rest = $content.Substring($classStart)
        $nextClass = [regex]::Match($rest.Substring(1), '(?m)^\s*public class ')
        $classBlock = if ($nextClass.Success) { $rest.Substring(0, $nextClass.Index + 1) } else { $rest }
        $fieldMatches = [regex]::Matches($classBlock, '(?m)^\s*public\s+([\w<>\.,\s\[\]]+)\s+(\w+)\s*;')
        $fields = @()

        foreach ($fieldMatch in $fieldMatches) {
            $rawType = $fieldMatch.Groups[1].Value.Trim()
            $fields += [pscustomobject]@{
                Type = (Resolve-TypeName $rawType $namespace $usings $typeRegistry)
                Name = $fieldMatch.Groups[2].Value
            }
        }

        $componentTypes.Add([pscustomobject]@{
            Name = $className
            FullName = "$namespace.$className"
            Fields = $fields
        }) | Out-Null
    }
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine('namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore')
[void]$sb.AppendLine('{')
[void]$sb.AppendLine("`tpublic partial class Entity")
[void]$sb.AppendLine("`t{")

foreach ($component in ($componentTypes | Sort-Object Name)) {
    $componentName = Remove-SuffixIfExists $component.Name "Component"
    $componentNameC = "${componentName}C"
    $fullTypeName = $component.FullName
    $fields = $component.Fields

    [void]$sb.AppendLine("`t`tpublic $fullTypeName $componentNameC => GetComponent<$fullTypeName>();")
    [void]$sb.AppendLine('')

    if ($fields.Count -eq 1 -and $fields[0].Name -eq 'Value') {
        $field = $fields[0]
        [void]$sb.AppendLine("`t`tpublic $($field.Type) $componentName => ${componentName}C.$($field.Name);")
        [void]$sb.AppendLine('')
        $varName = Get-VariableName $field.Name
        [void]$sb.AppendLine("`t`tpublic bool TryGet$componentName(out $($field.Type) $varName)")
        [void]$sb.AppendLine("`t`t{")
        [void]$sb.AppendLine("`t`t`tbool result = TryGetComponent(out $fullTypeName component);")
        [void]$sb.AppendLine("`t`t`tif(result)")
        [void]$sb.AppendLine("`t`t`t`t$varName = component.$($field.Name);")
        [void]$sb.AppendLine("`t`t`telse")
        [void]$sb.AppendLine("`t`t`t`t$varName = default($($field.Type));")
        [void]$sb.AppendLine("`t`t`treturn result;")
        [void]$sb.AppendLine("`t`t}")
        [void]$sb.AppendLine('')

        if ($field.Type -match 'ReactiveVariable<' -or $field.Type -match 'ReactiveEvent<' -or $field.Type -match 'List<') {
            $inner = if ($field.Type -match '<(.+)>$') { $Matches[1] } else { '' }
            $initializer = if ($field.Type -match 'ReactiveVariable<') { " { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveVariable<$inner>() }" }
                         elseif ($field.Type -match 'ReactiveEvent<') { " { Value = new Assets._Project.Develop.Runtime.Utilities.Reactive.ReactiveEvent<$inner>() }" }
                         elseif ($field.Type -match 'List<') { " { Value = new System.Collections.Generic.List<$inner>() }" }
                         else { '()' }
            [void]$sb.AppendLine("`t`tpublic Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity Add$componentName()")
            [void]$sb.AppendLine("`t`t{")
            [void]$sb.AppendLine("`t`t`treturn AddComponent(new $fullTypeName()$initializer); ")
            [void]$sb.AppendLine("`t`t}")
            [void]$sb.AppendLine('')
        }

        [void]$sb.AppendLine("`t`tpublic Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity Add$componentName($($field.Type) $(Get-VariableName $field.Name))")
        [void]$sb.AppendLine("`t`t{")
        [void]$sb.AppendLine("`t`t`treturn AddComponent(new $fullTypeName() {$($field.Name) = $(Get-VariableName $field.Name)}); ")
        [void]$sb.AppendLine("`t`t}")
        [void]$sb.AppendLine('')
    }
    else {
        $params = ($fields | ForEach-Object { "$($_.Type) $(Get-VariableName $_.Name)" }) -join ', '
        $init = ($fields | ForEach-Object { "$($_.Name) = $(Get-VariableName $_.Name)" }) -join ', '
        [void]$sb.AppendLine("`t`tpublic Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Entity Add$componentName($params)")
        [void]$sb.AppendLine("`t`t{")
        [void]$sb.AppendLine("`t`t`treturn AddComponent(new $fullTypeName() {$init}); ")
        [void]$sb.AppendLine("`t`t}")
        [void]$sb.AppendLine('')
    }
}

[void]$sb.AppendLine("`t}")
[void]$sb.AppendLine('}')

$outputPath = Join-Path $ProjectRoot "Assets\_Project\Develop\Runtime\Gameplay\EntitiesCore\Generated\EntityAPI.cs"
[System.IO.File]::WriteAllText($outputPath, $sb.ToString())
Write-Output "Generated $outputPath with $($componentTypes.Count) components"
