TBExt_configRecursive = {
    params ["_key", "_config", ["_path", []], ["_modus", "add"], ["_depth", 2]];
    if (count _path > _depth) exitWith {};
    
    {
        ([_x, true] call BIS_fnc_returnParents) params ["_subConfig", ["_subConfigParent", "<noParent>"]];
        private _subPath = +_path;
        _subPath pushBack _subConfig;
        
        if (_modus == "add") then
        {
            "TBModExtensionHost" callExtension ["inheritance", ["addEntry", [_key, _subPath joinString " >> ", _subConfigParent]]];
        }
        else
        {
            private _checkResult = "TBModExtensionHost" callExtension ["inheritance", ["checkEntry", [_key, _subPath joinString " >> ", _subConfigParent]]];
            if (_checkResult param [1, -1] == -1) then
            {
                diag_log format ["TBExt_checkConfig - Fehler bei: %1 -> %2 => %3", _subPath joinString " >> ", _subConfigParent, _checkResult param [0, "<NO_RESULT>"]];
            };
        };
        
        [_key, _x, _subPath, _modus, _depth] call TBExt_configRecursive;
    }
    forEach (configProperties [_config, "isClass _x", true]);
};

TBExt_addConfig = {
    params ["_config", ["_clear", false]];
    if (isNil "TBExt_loaded") then {TBExt_loaded = "TBModExtensionHost" callExtension "-"};
    if (_clear) then {"TBModExtensionHost" callExtension ["inheritance", ["clear"]]};
    [configName _config, _config, [configName _config], "add"] call TBExt_configRecursive;
    "TBModExtensionHost" callExtension ["inheritance", ["savefile"]];
};

TBExt_checkConfig = {
    params ["_config"];
    if (isNil "TBExt_loaded") then {TBExt_loaded = "TBModExtensionHost" callExtension "-"};
    diag_log format ["TBExt_checkConfig: %1", configName _config];
    "TBModExtensionHost" callExtension ["inheritance", ["loadfile"]];
    [configName _config, _config, [configName _config], "check"] call TBExt_configRecursive;
    "DONE"
};
diag_log "FunctionsTest loaded...";
