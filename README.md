# RecursiveNestedGroupSearch

Class that allows recursively searching an LDAP directory to find all groups
to which a user belongs.  This accounts for the fact that LDAP directory trees
can have multiple routes to the same group and can have loops.

This is a .Net 5 test project.  Clone in Visual Studio and run the tests in
the test file to try out the class.
