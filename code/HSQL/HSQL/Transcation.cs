using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace HSQL
{
    public class Transcation
    {
        private TransactionScope Scope;

        public void Invoke(Action action)
        {
            Scope = new TransactionScope();
            try
            {
                action();
                Commit();
            }
            catch (Exception ex)
            {
                RollBack();
            }
        }

        public void Commit()
        {
            Scope.Complete();
        }

        public void RollBack()
        {
            Scope.Dispose();
        }
    }
}
