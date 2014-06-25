NUnitAllureAdapter
==================

##Usage##

 - Clone repository and build solution;
 - Place output **\*.dll** files into **%NUnit\_Installation\_Directory%\bin\addins**;
 - Start **NUnit GUI**;
 - **NOTE:** addin will NOT be visible in **Tools -> Addins..** because it's built against .NET 4.0;
 - Start your tests.

When your tests finish, you'll see **AllureResults** folder, where output **xml**-files will be generated.
