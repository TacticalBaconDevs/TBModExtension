"TBModExtensionHost" callExtension "-";


hint "############################"
hint "MainExtension FNCs"
hint "############################"
"TBModExtensionHost" callExtension ["status", [1]];
"TBModExtensionHost" callExtension ["testleer", [1,2,3]];
"TBModExtensionHost" callExtension ["status", [1]];
"TBModExtensionHost" callExtension ["check", ["TBModExtension_Logging"]];
"TBModExtensionHost" callExtension ["check", ["logginG"]];
"TBModExtensionHost" callExtension ["teststring", [1,"test ""Test123"" huhu",2,'ha "warum" dashier',3]];


hint "############################"
hint "AsncTests";
hint "############################"
"TBModExtensionHost" callExtension ["testleer", []];
"TBModExtensionHost" callExtension ["status", [1]];

"TBModExtensionHost" callExtension ["failtest", []];
"TBModExtensionHost" callExtension ["status", [2]];

"TBModExtensionHost" callExtension ["callbacktest", []];
"TBModExtensionHost" callExtension ["status", [3]];

"TBModExtensionHost" callExtension ["waittest", []];
"TBModExtensionHost" callExtension ["status", [4]];
sleep 12;
"TBModExtensionHost" callExtension ["status", [4]];


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
"TBModExtensionHost" callExtension ["check", ["TBModExtension_Inheritance"]];
"TBModExtensionHost" callExtension ["check", ["inheritance"]];
"TBModExtensionHost" callExtension ["inheritance", ["loadfile"]];
"TBModExtensionHost" callExtension ["inheritance", ["clear"]];
"TBModExtensionHost" callExtension ["inheritance", ["addentry", ["cfg123", "_value", "_parent"]]];
"TBModExtensionHost" callExtension ["inheritance", ["savefile"]];
"TBModExtensionHost" callExtension ["inheritance", ["checkentry", ["cfg123", "_value", "_parent"]]];
"TBModExtensionHost" callExtension ["inheritance", ["checkentry", ["cfg123", "_value", "_parent2"]]];


freeExtension "TBModExtensionHost"
