namespace Mánagarmr.Models.SubsonicAPI.InfoPack
{
    public class FolderListInfoPack
    {
        public FolderListInfoPack(string id, string name)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; set; }

        public string Id { get; set; }
    }
}