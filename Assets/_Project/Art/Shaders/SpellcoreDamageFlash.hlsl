#ifndef SPELLCORE_DAMAGE_FLASH_INCLUDED
#define SPELLCORE_DAMAGE_FLASH_INCLUDED

half3 ApplySpellcoreDamageFlash(half3 color, half3 flashColor, half flashAmount)
{
    half amount = saturate(flashAmount);
    half3 fill = lerp(color, flashColor, amount);
    half3 boost = flashColor * amount * 0.65h;
    return fill + boost;
}

#endif
