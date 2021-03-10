TBExt_addConfig = {
    params ['_config'];
    {
        ([_x, true] call BIS_fnc_returnParents) params ['_value', ['_parent', '<noParent>']];
        'TBModExtensionHost' callExtension ['inheritance', ['addEntry', [configName _config, _value, _parent]]];
    }
    forEach ('true' configClasses _config);
    'TBModExtensionHost' callExtension ['inheritance', ['savefile']];
};

TBExt_checkConfig = {
    params ['_config'];
    'TBModExtensionHost' callExtension ['inheritance', ['loadfile']];
    {
        ([_x, true] call BIS_fnc_returnParents) params ['_value', ['_parent', '<noParent>']];
        if ((('TBModExtensionHost' callExtension ['inheritance', ['checkEntry', [configName _config, _value, _parent]]]) param [1, -1]) == -1) then
        {
            diag_log format ['TBExt_checkConfig - Fehler bei: %1 -> %2', _value, _parent];
        };
    }
    forEach ('true' configClasses _config);
};
diag_log "FunctionsTest loaded...";
