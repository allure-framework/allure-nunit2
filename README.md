# Allure NUnit Adapter

## Installation and Usage

 1. Download NUnit **2.6.3** from [official site](http://www.nunit.org/);
 2. Download latest release from [allure-nunit releases](https://github.com/allure-framework/allure-nunit/releases);
 3. Unpack binaries to **%NUnit_installation_directory%\bin\addins**;
 4. **NOTE:** addin will NOT be visible in **Tools -> Addins..** because it's built against **.NET 4.0**;
 5. Run your tests with **NUnit GUI** or **nunit-console** using .NET 4.0 (e.g. nunit-console YourAssembly.dll /framework=net-4.0);
 6. After all tests finish you'll see **AllureResults** folder where **xml** files will be generated (you can specify path to results folder in **config.xml**);

## How to generate report
This adapter only generates XML files containing information about tests. See [wiki section](https://github.com/allure-framework/allure-core/wiki#generating-report) on how to generate report.

## Further reading
 * [Allure NUnit Wiki](https://github.com/allure-framework/allure-csharp-commons/wiki)
