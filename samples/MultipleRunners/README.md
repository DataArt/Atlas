# dotnet-atlas

ServiceFabric multiple runners solutions

- ConsoleAndFabricDynamic solution
  This solution illustrates aproach to use optional entry pont (main) command line argument.
  When you run this service from Service Fabric, ApplicationManifest.xml file supplies "Fabric"
  argument into the service to let it know it should be run in Service Fabric mode.
  If you specify no arguments it will run in Console mode by default.

- MSBuild solution
  This solution illustrates approach to use conditional compilation constant FABRIC to make sure
  ApplicationRunner built with necessary Atlas components for Fabric or Console deployment.
  FABRIC constant defined in service project file for DebugFabric/ReleaseFabric configurations.

- after.<SolutionName>.sln.targets
  This file will be used when doing Continuous Integration and run MSBuild from command line (not Visual Studio)
  to make sure Package action works correctly
