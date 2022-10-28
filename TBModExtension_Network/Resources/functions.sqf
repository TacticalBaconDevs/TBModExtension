TBExt_downloadString = {
    params ["_url"];
    private _result = "";
    
    if (!canSuspend) exitWith {"ERROR: Suspending not allowed in this contex"};

    ("TBModExtensionHost" callExtension ["network", ["#downloadString", _url]]) params ["", "_cacheId"];
    if (_cacheId > 0) then
    {
        waitUntil
        {
            ("TBModExtensionHost" callExtension ["host", ["status", _cacheId]]) params ["_status"];
            _status != "QUEUE"
        };
        
        waitUntil
        {
            ("TBModExtensionHost" callExtension ["network", ["getDownloadString", _cacheId]]) params ["_urlContent", "_anzahl"];
            _result = _result + _urlContent;
            _anzahl <= 1
        };
    };
    
    _result
};
