namespace FileSystemLibrary;


public class FolderItem : FileSystemItem
{

    public List<FileSystemItem> Children { get; } = new List<FileSystemItem>();

    public override FileSystemElementType ElementType => FileSystemElementType.Folder;

    public override long Size => Children.Sum(c => c.Size);


    public FolderItem(string name, FolderItem? parent = null)
        : base(name, parent)
    {
    }

    public void AddChild(FileSystemItem item)
    {
        Children.Add(item);
        item.ParentFolder = this;
    }

    public void RemoveChild(FileSystemItem item)
    {
        Children.Remove(item);
    }

    public override string ToString() => $"Folder: {Name}, Size: {Size}, Location: {Location}";
}
