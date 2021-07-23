/**
 * Copyright (c) 2021-present, MFB Technologies, Inc.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using System.Linq;

namespace RecursiveNestedGroupSearch
{
    public class LdapGroupGraph
    {
        private readonly Dictionary<string, List<string>> _adjacencyList;

        public LdapGroupGraph(IList<LdapEntry> groupLdapEntries, IList<LdapEntry> userLdapEntries)
        {
            _adjacencyList = new();
            foreach (var group in groupLdapEntries)
            {
                var (groupDN, listOfParentGroupDNs) = GetUserOrGroupDnAndMemberOf(group);
                _adjacencyList.Add(groupDN, listOfParentGroupDNs.ToList());
            }
            foreach (var user in userLdapEntries)
            {
                var (userDN, listOfParentGroupDNs) = GetUserOrGroupDnAndMemberOf(user);
                _adjacencyList.Add(userDN, listOfParentGroupDNs.ToList());
            }
        }

        public IEnumerable<string> RecursiveGroupList(LdapEntry groupOrUser)
        {
            var results = new HashSet<string>();
            var (userOrGroupDn, _) = GetUserOrGroupDnAndMemberOf(groupOrUser);
            GetGroupsRecursive(userOrGroupDn, ref results);
            return results;
        }

        private void GetGroupsRecursive(string startingGroupDN, ref HashSet<string> alreadyFound)
        {
            var groupEdges = _adjacencyList[startingGroupDN];
            if (groupEdges != null && groupEdges.Count > 0)
            {
                foreach (var group in groupEdges)
                {
                    if (!alreadyFound.Contains(group))
                    {
                        alreadyFound.Add(group);
                        GetGroupsRecursive(group, ref alreadyFound);
                    }
                }
            }
        }

        private static (string, IEnumerable<string>) GetUserOrGroupDnAndMemberOf(LdapEntry userOrGroupEntry)
        {
            return (userOrGroupEntry.DistinguishedName, userOrGroupEntry.MemberOf);
        }
    }
}
