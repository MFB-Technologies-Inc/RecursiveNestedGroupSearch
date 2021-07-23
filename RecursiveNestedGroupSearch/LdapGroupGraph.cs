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
        private readonly Dictionary<string, HashSet<string>> _fullMembershipLookup;

        public LdapGroupGraph(IList<LdapEntry> groupLdapEntries)
        {
            _adjacencyList = new();
            foreach (var group in groupLdapEntries)
            {
                var (groupDN, listOfParentGroupDNs) = GetUserOrGroupDnAndMemberOf(group);
                _adjacencyList.Add(groupDN, listOfParentGroupDNs.ToList());
            }

            // pre-fetch the full membership list for every group
            _fullMembershipLookup = new();
            foreach (var childGroup in _adjacencyList)
            {
                var fullGroupList = new HashSet<string>();
                GetGroupsRecursive(childGroup.Key, ref fullGroupList);
                _fullMembershipLookup.Add(childGroup.Key, fullGroupList);
            }
        }

        public IEnumerable<string> RecursiveGroupList(LdapEntry groupOrUser)
        {
            //grab the full group for each immediate parent (which has been pre-fetched)
            //and then just take the union to avoid duplicate groups
            var (_, startingGroupDNs) = GetUserOrGroupDnAndMemberOf(groupOrUser);
            var groupUnion = new HashSet<string>(startingGroupDNs);
            foreach (var groupDN in startingGroupDNs)
            {
                groupUnion.UnionWith(_fullMembershipLookup[groupDN]);
            }
            return groupUnion;
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
