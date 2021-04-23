using DiscUtils.Streams;
using DiscUtils.Vfs;
using System.IO;

namespace DiscUtils.HfsPlus
{
    internal class Symlink : File, IVfsSymlink<DirEntry, File>
    {
        private string targetPath;

        public Symlink(Context context, CatalogNodeId nodeId, CommonCatalogFileInfo catalogInfo)
            : base(context, nodeId, catalogInfo) {}

        public string TargetPath
        {
            get
            {
                if (this.targetPath == null)
                {
                    using (BufferStream stream = new BufferStream(this.FileContent, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        this.targetPath = reader.ReadToEnd();
                        this.targetPath = this.targetPath.Replace('/', '\\');
                    }
                }

                return this.targetPath;
            }
        }
    }
}