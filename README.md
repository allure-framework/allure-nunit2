NUnitAllureAdapter
==================

##Usage##

 - Download NUnit from [official site](http://www.nunit.org/);
 - Download latest release from [allure-nunit releases](https://github.com/allure-framework/allure-nunit/releases);
 - Unpack binaries to **%NUnit_installation_directory%\bin\addins**;
 - **NOTE:** addin will NOT be visible in **Tools -> Addins..** because it's built against **.NET 4.0**;
 - Run your tests with **NUnit GUI** or **nunit-console**;
 - After all tests finish you'll see **AllureResults** folder where **xml** files will be generated (you can specify relative path to results folder in **config.xml**);
 - Use [allure-cli](https://github.com/allure-framework/allure-core/tree/master/allure-cli) to generate report from theese **xml** files;
