/**
 * Copyright (c) 2021-present, MFB Technologies, Inc.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecursiveNestedGroupSearch
{
    public class LdapEntry
    {
        public string Dn { get; set; }
        public string DistinguishedName { get; set; }
        public List<string> MemberOf { get; set; }
    }
}
