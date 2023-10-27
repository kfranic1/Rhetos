# Rhetos - A DSL platform

Rhetos is a DSL platform for Enterprise Application Development.
It extends the modern .NET technology stack with advanced programming paradigms: declarative programming, metaprogramming and AOP.

* Rhetos enables developers to create a **Domain-Specific Programming Language** and use it to write their applications.
* There are libraries available with ready-to-use implementations of many standard business and design patterns or technology integrations.

Rhetos works as a compiler that generates C# code, SQL, and other source files, from the application model written in the DSL scripts.

* The generated application is a standard business application based on Microsoft .NET technology stack.
* Rhetos is focused on the back-end development: It generates the business logic layer (C# object model), the database and the web API (REST, SOAP, etc.).
* The database is not generated from scratch on each deployment, it is upgraded instead, protecting the existing data.

Rhetos comes with the *CommonConcepts* DSL package, a programming language extension that contains many ready-to-use features for building applications.

[IntelliSense](https://github.com/Rhetos/LanguageServices/blob/master/README.md)
and [syntax highlighting](https://github.com/Rhetos/Rhetos/wiki/Prerequisites#configure-your-text-editor-for-dsl-scripts-rhe)
is available for Visual Studio, Visual Studio Code, SublimeText3 and Notepad++.

## Documentation and samples

See [Rhetos wiki](https://github.com/Rhetos/Rhetos/wiki) for more information on:

* Creating applications with Rhetos framework
* Rhetos DSL examples
* Available plugins
* Principles behind the [Rhetos platform](https://github.com/Rhetos/Rhetos/wiki/What-is-Rhetos)
* [Prerequisites, Installation and Development Environment](https://github.com/Rhetos/Rhetos/wiki/Development-Environment-Setup)

Visit the project web site at [rhetos.org](http://www.rhetos.org/).

## License

The code in this repository is licensed under version 3 of the AGPL unless
otherwise noted.
Please see [License.txt](License.txt) for details.

## How to contribute

Contributions are very welcome. The easiest way is to fork this repo, and then
make a pull request from your fork. The first time you make a pull request, you
may be asked to sign a Contributor Agreement.
For more info see [How to Contribute](https://github.com/Rhetos/Rhetos/wiki/How-to-Contribute) on Rhetos wiki.

### Building the source code

Note: Rhetos NuGet packages are already available at the [NuGet.org](https://www.nuget.org/) online gallery.
You don't need to build it from source in order to use it in your application.

To build the source, run `Clean.bat` and `Build.bat`.
The build output files are NuGet packages in the `dist` subfolder.

### Testing

Initial setup (required for integration tests):

* Create an empty database (for example, "Rhetos" database on "localhost" SQL Server instance).
* Create a local setting file `test\CommonConcepts.TestApp\local.settings.json` with the following content,
   and modify the connection string to match your server instance and database:
    ```json
    {
      "ConnectionStrings": {
        "RhetosConnectionString": "Data Source=ENTER_SQL_SERVER_NAME;Initial Catalog=ENTER_RHETOS_DATABASE_NAME;Integrated Security=true;"
      }
    }
    ```

To execute the unit tests and the integration tests, run `Test.bat`.

### Visual Studio Solutions

**Rhetos.sln** contains the source for Rhetos framework and CommonConcepts plugins (a standard library for Rhetos DSL).
It also contains unit tests for the projects.

**CommonConceptsTest.sln** contains the integration tests for DSL concepts in CommonConcepts.
After changing the framework code in Rhetos.sln, you will need to run Build.bat and Test.bat,
*before* you can develop the related integration tests in CommonConceptsTest.sln.
CommonConceptsTest depends on Rhetos NuGet packages that are created and provide by those scripts.
