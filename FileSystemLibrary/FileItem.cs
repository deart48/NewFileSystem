namespace FileSystemLibrary;


public class FileItem : FileSystemItem
{

    public long FileSize { get; }

    public override FileSystemElementType ElementType => FileSystemElementType.File;


    public override long Size => FileSize;

    public FileItem(string name, long fileSize)
        : base(name, null)
    {
        FileSize = fileSize;
    }

    public override string ToString() => $"File: {Name}, Size: {Size}, Location: {Location}";
}
