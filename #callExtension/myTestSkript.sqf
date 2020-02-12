#define freeExtension comment

freeExtension "test";
"TBModExtension" callExtension "-";

"TBModExtension" callExtension ["status", [1]];
"TBModExtension" callExtension ["testleer", [1,2,3]];
"TBModExtension" callExtension ["status", [1]];

hint "";

"TBModExtension" callExtension ["check", ["TBModExtension_Logging"]];
"TBModExtension" callExtension ["check", ["logginG"]];

hint "";

//"TBModExtension" callExtension ["registerlogger", ["logger", 1]];
//"TBModExtension" callExtension ["registerlogger", ["logger", "TestLog.log"]];

//"TBModExtension" callExtension ["logger", ["logger", "INFO", 1]];
//"TBModExtension" callExtension ["logger", ["logger", "INFO", "Test123"]];

//"TBModExtension" callExtension ["testleer", []];
//"TBModExtension" callExtension ["status", [1]];

//"TBModExtension" callExtension ["failtest", []];
//"TBModExtension" callExtension ["status", [2]];

//"TBModExtension" callExtension ["callbacktest", []];
//"TBModExtension" callExtension ["status", [3]];

//"TBModExtension" callExtension ["waittest", []];
//"TBModExtension" callExtension ["status", [4]];
//sleep 12;
//"TBModExtension" callExtension ["status", [4]];
//sleep 30;
//exit
