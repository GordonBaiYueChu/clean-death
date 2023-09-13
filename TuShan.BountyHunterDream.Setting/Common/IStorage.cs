namespace TuShan.BountyHunterDream.Setting.Common
{
    public interface IStorage<T>
    {
        T Read(string FileName);

        void Write(T t, string FileName);
    }
}
