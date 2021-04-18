cd ..

::copy /y "TBModExtensionPlugin\bin\Debug\netstandard2.0\TBModExtensionPlugin.dll" #callExtension
::copy /y "TBModExtensionPlugin\bin\Debug\netstandard2.0\TBModExtensionPlugin.pdb" #callExtension

copy /y "TBModExtensionHost\bin\Debug\netstandard2.0\TBModExtensionHost_x64.dll" #callExtension
copy /y "TBModExtensionHost\bin\Debug\netstandard2.0\TBModExtensionHost_x64.pdb" #callExtension

copy /y "TBModExtension_Inheritance\bin\Debug\netstandard2.0\TBModExtension_Inheritance.dll" #callExtension
copy /y "TBModExtension_Inheritance\bin\Debug\netstandard2.0\TBModExtension_Inheritance.pdb" #callExtension

copy /y "TBModExtension_Network\bin\Debug\netstandard2.0\TBModExtension_Network.dll" #callExtension
copy /y "TBModExtension_Network\bin\Debug\netstandard2.0\TBModExtension_Network.pdb" #callExtension

copy /y "TBModExtension_Logging\bin\Debug\netstandard2.0\TBModExtension_Logging.dll" #callExtension
copy /y "TBModExtension_Logging\bin\Debug\netstandard2.0\TBModExtension_Logging.pdb" #callExtension

cd #certStuff
call signDll.cmd
cd ..

cd #callExtension
