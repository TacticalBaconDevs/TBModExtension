"TBModExtensionHost" callExtension "-";

hint "############################"
hint "MainExtension FNCs"
hint "############################"
"TBModExtensionHost" callExtension ["host", ["status", 1]];
"TBModExtensionHost" callExtension ["host", ["check", "logging"]];
"TBModExtensionHost" callExtension ["host", ["check", "TBModExtension_inheritance"]];
"TBModExtensionHost" callExtension ["host", ["check", "pluginUnknown"]];
"TBModExtensionHost" callExtension ["host", ["check", "host"]];
//"TBModExtensionHost" callExtension ["host", ["teststring", [1, "test ""Test123"" huhu", 2, 'ha "warum" dashier', 3]]];
"TBModExtensionHost" callExtension ["host", ["teststring", "benötige Infos"]];


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


hint "############################"
systemChat "Logging";
hint "############################"
"TBModExtensionHost" callExtension ["logging", ["register", ["TestLogger", "#TestLog.log"]]];
"TBModExtensionHost" callExtension ["logging", ["#log", ["TestLogger", "TEST", "Test123"]]];
"TBModExtensionHost" callExtension ["logging", ["#log", ["TestLogger", "TEST", "Test456"]]];
"TBModExtensionHost" callExtension ["logging", ["#log", ["TestLogger", "PVP_KILLED", "Gen. Shukari(B_Soldier_lite_F) von Gen. Shukari(B_Soldier_lite_F) durch #scripted  --->  [bob2,""#scripted"",bob2,<NULL-object>]"]]]



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
"TBModExtensionHost" callExtension ["network", ["#downloadString", "https://raw.githubusercontent.com/TacticalBaconDevs/MainCore-Layout/master/TB_MAINCORE.VR/stuff/defaultSettings.txt"]];
"TBModExtensionHost" callExtension ["host", ["getCache", 1]];

freeExtension "TBModExtensionHost"
