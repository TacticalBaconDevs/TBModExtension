"TBModExtensionHost" callExtension "-";

hint "############################"
hint "MainExtension FNCs"
hint "############################"
"TBModExtensionHost" callExtension ["host", ["status", 1]];
"TBModExtensionHost" callExtension ["host", ["check", "TBModExtension_Inheritance"]];
"TBModExtensionHost" callExtension ["host", ["check", "inheritance"]];
"TBModExtensionHost" callExtension ["host", ["check", "pluginUnknown"]];
"TBModExtensionHost" callExtension ["host", ["check", "host"]];
"TBModExtensionHost" callExtension ["host", ["teststring", [1, "test ""Test123"" huhu", 2, 'ha "warum" dashier', 3]]];


hint "############################"
hint "AsncTests";
hint "############################"
"TBModExtensionHost" callExtension ["host", ["status", 1]];

"TBModExtensionHost" callExtension ["host", ["#failtest"]];
"TBModExtensionHost" callExtension ["host", ["status", 1]];

"TBModExtensionHost" callExtension ["host", ["#callbacktest"]];
"TBModExtensionHost" callExtension ["host", ["status", 2]];

"TBModExtensionHost" callExtension ["host", ["#waittest"]];
"TBModExtensionHost" callExtension ["host", ["status", 3]];
sleep 4;
"TBModExtensionHost" callExtension ["host", ["status", 3]];


//hint "############################"
//systemChat "Logger";
//hint "############################"
//"TBModExtensionHost" callExtension ["registerlogger", ["logger", 1]];
//"TBModExtensionHost" callExtension ["registerlogger", ["logger", "TestLog.log"]];
//"TBModExtensionHost" callExtension ["logger", ["logger", "INFO", 1]];
//"TBModExtensionHost" callExtension ["logger", ["logger", "INFO", "Test123"]];


hint "############################"
hint "inheritance";
hint "############################"
"TBModExtensionHost" callExtension ["host", ["check", "TBModExtension_Inheritance"]];
"TBModExtensionHost" callExtension ["host", ["check", "inheritance"]];
"TBModExtensionHost" callExtension ["inheritance", ["loadfile"]];
"TBModExtensionHost" callExtension ["inheritance", ["clear"]];
"TBModExtensionHost" callExtension ["inheritance", ["addentry", ["cfg123", "_value", "_parent"]]];
"TBModExtensionHost" callExtension ["inheritance", ["savefile"]];
"TBModExtensionHost" callExtension ["inheritance", ["checkentry", ["cfg123", "_value", "_parent"]]];
"TBModExtensionHost" callExtension ["inheritance", ["checkentry", ["cfg123", "_value", "_parent2"]]];


hint "############################"
hint "network";
hint "############################"
"TBModExtensionHost" callExtension "-";
"TBModExtensionHost" callExtension ["network", ["#downloadString", "https://raw.githubusercontent.com/TacticalBaconDevs/MainCore-Layout/master/TB_MAINCORE.VR/stuff/defaultSettings.txt"]];
"TBModExtensionHost" callExtension ["network", ["getDownloadString", 1]];

freeExtension "TBModExtensionHost"
