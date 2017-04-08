namespace RiakTEF.Profiles
{
    public interface IProfile
    {
        void Register(ISchema schema);
    }

    public abstract class Profile<T> : IProfile
    {
        public void Register(ISchema schema)
        {
            Register(schema.Entity<T>());
        }

        protected abstract void Register(IEntity<T> entity);
    }
}
