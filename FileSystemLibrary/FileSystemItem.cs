namespace FileSystemLibrary;

public abstract class FileSystemItem : IFileSystemElement
{

    public string Name { get; set; }


    public FolderItem? ParentFolder { get; set; }


    public string Location
    {
        get
        {
            if (ParentFolder == null)
                return Name;
            return ParentFolder.Location + "/" + Name;
        }
    }

    public abstract FileSystemElementType ElementType { get; }

    public abstract long Size { get; }

    protected FileSystemItem(string name, FolderItem? parent = null)
    {
        Name = name;
        ParentFolder = parent;
    }

    public static void Copy(FileSystemItem item, FolderItem destination)
    {
        destination.AddChild(item);
    }

    public static void Move(FileSystemItem item, FolderItem destination)
    {
        item.ParentFolder?.RemoveChild(item);
        destination.AddChild(item);
    }
}
