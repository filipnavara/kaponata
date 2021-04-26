using DiscUtils.HfsPlus;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    public class BTreeIndexNodeTests
    {
        [Fact]
        public void ReadFrom()
        {
            var node = new BTreeIndexNode<CatalogKey>(
                Mock.Of<BTree>(),
                new BTreeNodeDescriptor());

            node.ReadFrom(File.ReadAllBytes("HfsPlus/indexnode.bin"), 0);
        }
    }
}
