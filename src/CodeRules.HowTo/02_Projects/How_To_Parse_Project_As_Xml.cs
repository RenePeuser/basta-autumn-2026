using System.Xml.Linq;
using AspNetCore.Simple.MsTest.Sdk;

namespace CodeRules.HowTo
{
    [TestClass]
    [TestCategory("HowTo")]
    public class Parse_Project_As_Xml_Test
    {
        private static readonly string ProjectPath = RepositoryPaths.HowToProject;

        [TestMethod]
        public void How_To_Load_A_Project_File()
        {
            var doc = XDocument.Load(ProjectPath);

            Assert.That.IsNotNull(doc,
                                  because: "XDocument.Load must return a valid XML document",
                                  fix: "Check if project file exists and is valid XML");

            Assert.That.IsNotNull(doc.Root,
                                  because: "Project file must have a root element",
                                  fix: "Ensure project file has <Project> root element");

            Assert.That.AreEqual("Project", doc.Root.Name.LocalName,
                                 because: "Root element must be 'Project'",
                                 fix: "Fix the root element name in the .csproj file");
        }

        [TestMethod]
        public void How_To_Find_PackageReferences()
        {
            var doc = XDocument.Load(ProjectPath);

            Assert.That.IsNotNull(doc.Root,
                                  because: "Project must have root element",
                                  fix: "Check project file structure");

            var packages = doc.Root
                              .Descendants("PackageReference")
                              .ToList();

            Assert.That.IsNotEmpty(packages,
                                   because: "Project should have at least one PackageReference",
                                   fix: "Add package references to the project");

            var firstPackage = packages[0];
            var packageName = firstPackage.Attribute("Include")?.Value;

            Assert.That.IsNotNull(packageName,
                                  because: "PackageReference must have Include attribute",
                                  fix: "Add Include attribute with package name");

            Assert.That.IsNotEmpty(packageName,
                                   because: "Package name must not be empty",
                                   fix: "Set a valid package name in Include attribute");
        }
    }
}
