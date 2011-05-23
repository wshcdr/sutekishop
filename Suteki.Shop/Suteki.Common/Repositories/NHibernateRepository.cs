using System.Linq;
using NHibernate.Linq;

namespace Suteki.Common.Repositories
{
    public class NHibernateRepository<T> : IRepository<T>, IRepository where T : class
    {
        readonly ISessionManager sessionManager;

        public NHibernateRepository(ISessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
        }

        public T GetById(int id)
        {
            return sessionManager.OpenSession().Get<T>(id);
        }

        public IQueryable<T> GetAll()
        {
            return sessionManager.OpenSession().Query<T>();
        }

        public void SaveOrUpdate(T entity)
        {
            sessionManager.OpenSession().SaveOrUpdate(entity);
        }

        public void DeleteOnSubmit(T entity)
        {
            sessionManager.OpenSession().Delete(entity);
        }

        object IRepository.GetById(int id)
        {
            return GetById(id);
        }

        IQueryable IRepository.GetAll()
        {
            return GetAll();
        }

        void IRepository.SaveOrUpdate(object entity)
        {
            SaveOrUpdate((T)entity);
        }

        void IRepository.DeleteOnSubmit(object entity)
        {
            DeleteOnSubmit((T)entity);
        }
    }
}