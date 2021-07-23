/**
 * Copyright (c) 2021-present, MFB Technologies, Inc.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecursiveNestedGroupSearch
{
    [TestClass]
    public class LdapGroupGraphTests
    {
        [TestMethod]
        public void ReturnsImmediateParentsIfNotNested()
        {
            string user1 = Guid.NewGuid().ToString();
            string group1 = Guid.NewGuid().ToString();
            string group2 = Guid.NewGuid().ToString();
            string group3 = Guid.NewGuid().ToString();

            var userImmediateParents = new List<string> { group1, group2, group3 };

            var graph = new Dictionary<(LDAP, string), List<string>>();
            graph.Add((LDAP.User, user1), userImmediateParents);
            graph.Add((LDAP.Group, group1), new List<string>() );
            graph.Add((LDAP.Group, group2), new List<string>() );
            graph.Add((LDAP.Group, group3), new List<string>() );

            var directory = CreateLdapDirectoryFromGraph(graph);

            var groupGraph = new LdapGroupGraph(directory.GroupEntries);

            var user1Entry = directory.UserEntries.Find(entry => entry.DistinguishedName==user1);

            var groupList = groupGraph.RecursiveGroupList(user1Entry);
            Assert.AreEqual(3, groupList.Count());
            foreach (var group in groupList)
            {
                Assert.IsTrue(userImmediateParents.Contains(group));
            }
        }

        [TestMethod]
        public void RecursivelyReturnsAllParentsIfNested()
        {
            string user1 = Guid.NewGuid().ToString();
            var groups = new List<string>();
            for(var i = 0; i < 10; i++)
            {
                groups.Add(Guid.NewGuid().ToString());
            }

            var graph = new Dictionary<(LDAP, string), List<string>>();
            graph.Add((LDAP.User, user1), new List<string> { groups[0], groups[1], groups[2] });
            graph.Add((LDAP.Group, groups[0]), new List<string> { groups[3], groups[4] } );
            graph.Add((LDAP.Group, groups[1]), new List<string>() );
            graph.Add((LDAP.Group, groups[2]), new List<string> { groups[5] } );
            graph.Add((LDAP.Group, groups[3]), new List<string> { groups[6] } );
            graph.Add((LDAP.Group, groups[4]), new List<string> { groups[7] } );
            graph.Add((LDAP.Group, groups[5]), new List<string>() );
            graph.Add((LDAP.Group, groups[6]), new List<string>() );
            graph.Add((LDAP.Group, groups[7]), new List<string>() );
            graph.Add((LDAP.Group, groups[8]), new List<string> { groups[9] } );
            graph.Add((LDAP.Group, groups[9]), new List<string>() );

            var expectedMembership = new List<string> { groups[0], groups[1], groups[2], groups[3], groups[4], groups[5], groups[6], groups[7] };

            var directory = CreateLdapDirectoryFromGraph(graph);

            var groupGraph = new LdapGroupGraph(directory.GroupEntries);

            var user1Entry = directory.UserEntries.Find(entry => entry.DistinguishedName==user1);

            var groupList = groupGraph.RecursiveGroupList(user1Entry);
            Assert.AreEqual(8, groupList.Count());
            foreach (var group in groupList)
            {
                Assert.IsTrue(expectedMembership.Contains(group));
            }
        }

        [TestMethod]
        public void DoesNotReturnDuplicateParentsIfMultipleRoutesToParent()
        {
            string user1 = Guid.NewGuid().ToString();
            var groups = new List<string>();
            for(var i = 0; i < 10; i++)
            {
                groups.Add(Guid.NewGuid().ToString());
            }

            var graph = new Dictionary<(LDAP, string), List<string>>();
            graph.Add((LDAP.User, user1), new List<string> { groups[0], groups[1], groups[2] });
            graph.Add((LDAP.Group, groups[0]), new List<string> { groups[3], groups[4], groups[5] } );
            graph.Add((LDAP.Group, groups[1]), new List<string>() );
            graph.Add((LDAP.Group, groups[2]), new List<string> { groups[5], groups[3] } );
            graph.Add((LDAP.Group, groups[3]), new List<string> { groups[6] } );
            graph.Add((LDAP.Group, groups[4]), new List<string> { groups[7] } );
            graph.Add((LDAP.Group, groups[5]), new List<string>() );
            graph.Add((LDAP.Group, groups[6]), new List<string> { groups[5] } );
            graph.Add((LDAP.Group, groups[7]), new List<string>() );
            graph.Add((LDAP.Group, groups[8]), new List<string> { groups[9] } );
            graph.Add((LDAP.Group, groups[9]), new List<string>() );

            var expectedMembership = new List<string> { groups[0], groups[1], groups[2], groups[3], groups[4], groups[5], groups[6], groups[7] };

            var directory = CreateLdapDirectoryFromGraph(graph);

            var groupGraph = new LdapGroupGraph(directory.GroupEntries);

            var user1Entry = directory.UserEntries.Find(entry => entry.DistinguishedName==user1);

            var groupList = groupGraph.RecursiveGroupList(user1Entry);
            Assert.AreEqual(8, groupList.Count());
            foreach (var group in groupList)
            {
                Assert.IsTrue(expectedMembership.Contains((group)));
            }
        }

        [TestMethod]
        public void DoesNotReturnDuplicatesOrInfinitelyLoopOnLoopingRoutes()
        {
            string user1 = Guid.NewGuid().ToString();
            var groups = new List<string>();
            for(var i = 0; i < 10; i++)
            {
                groups.Add(Guid.NewGuid().ToString());
            }

            var graph = new Dictionary<(LDAP, string), List<string>>();
            graph.Add((LDAP.User, user1), new List<string> { groups[0], groups[1], groups[2] });
            graph.Add((LDAP.Group, groups[0]), new List<string> { groups[3], groups[4] } );
            graph.Add((LDAP.Group, groups[1]), new List<string>() );
            graph.Add((LDAP.Group, groups[2]), new List<string> { groups[5], groups[3] } );
            graph.Add((LDAP.Group, groups[3]), new List<string> { groups[6] } );
            graph.Add((LDAP.Group, groups[4]), new List<string> { groups[7], groups[0] } );
            graph.Add((LDAP.Group, groups[5]), new List<string>() );
            graph.Add((LDAP.Group, groups[6]), new List<string>() );
            graph.Add((LDAP.Group, groups[7]), new List<string> { groups[4] } );
            graph.Add((LDAP.Group, groups[8]), new List<string> { groups[9] } );
            graph.Add((LDAP.Group, groups[9]), new List<string>() );

            var expectedMembership = new List<string> { groups[0], groups[1], groups[2], groups[3], groups[4], groups[5], groups[6], groups[7] };

            var directory = CreateLdapDirectoryFromGraph(graph);

            var groupGraph = new LdapGroupGraph(directory.GroupEntries);

            var user1Entry = directory.UserEntries.Find(entry => entry.DistinguishedName==user1);

            var groupList = groupGraph.RecursiveGroupList(user1Entry);
            Assert.AreEqual(8, groupList.Count());
            foreach (var group in groupList)
            {
                Assert.IsTrue(expectedMembership.Contains((group)));
            }
        }
        
        private enum LDAP
        {
            User = 0,
            Group = 1
        }

        private class LdapDirectory
        {
            public List<LdapEntry> UserEntries { get; }
            public List<LdapEntry> GroupEntries { get; } 

            public LdapDirectory(List<LdapEntry> userEntries, List<LdapEntry> groupEntries)
            {
                UserEntries = userEntries;
                GroupEntries = groupEntries;
            }

        }

        private LdapDirectory CreateLdapDirectoryFromGraph(IDictionary<(LDAP, string),List<string>> adjacencyList)
        {
            LdapDirectory ldapDirectory = new(new(), new());

            foreach(var ((entryType, entryName), entryMemberOf) in adjacencyList)
            {
                if (entryType == LDAP.User)
                {
                    ldapDirectory.UserEntries.Add(CreateLdapUser(entryName, entryMemberOf));
                }
                if (entryType == LDAP.Group)
                {
                    ldapDirectory.GroupEntries.Add(CreateLdapGroup(entryName, entryMemberOf));
                }
            }

            return ldapDirectory;
        }
        private LdapEntry CreateLdapGroup(string groupCN, IEnumerable<string> memberOfCNs)
        {
            return new LdapEntry
            {
                Dn = "group",
                DistinguishedName = groupCN,
                MemberOf = memberOfCNs.ToList()
            };
        }

        private LdapEntry CreateLdapUser(string userAccountName, IEnumerable<string> memberOfCNs)
        {
            return new LdapEntry
            {
                Dn = "user",
                DistinguishedName = userAccountName,
                MemberOf = memberOfCNs.ToList()
            };
        }

    }

}
