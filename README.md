# Allure NUnit Adapter

## Installation and Usage

 1. Download NUnit **2.6.3** from [official site](http://www.nunit.org/);
 2. Download latest release from [allure-nunit releases](https://github.com/allure-framework/allure-nunit/releases);
 3. Unpack binaries to **%NUnit_installation_directory%\bin\addins**;
 4. **NOTE:** addin will NOT be visible in **Tools -> Addins..** because it's built against **.NET 4.0**;
 5. In **%NUnit_installation_directory%\bin\addins\config.xml** specify ABSOLUTE path to any folder (this folder will be created automatically) where **xml** files will be generated (e.g. **&lt;results-path>C:\test-results\AllureResults&lt;/results-path>** or **&lt;results-path>/home/user/test-results/AllureResults&lt;/results-path>**);
 6. You can also specify in configuration whether you want to take screenshots after failed tests and whether you want to have test output to be written to attachments.
 7. Run your tests with **NUnit GUI** or **nunit-console** using .NET 4.0 (e.g. nunit-console YourAssembly.dll /framework=net-4.0);
 8. After all tests finish you'll see new folder that you specified on step 5 with generated **xml** files;

## How to generate report
This adapter only generates XML files containing information about tests. See [wiki section](https://github.com/allure-framework/allure-core/wiki#generating-report) on how to generate report.

## Further reading
 * [Allure NUnit Wiki](https://github.com/allure-framework/allure-csharp-commons/wiki)
