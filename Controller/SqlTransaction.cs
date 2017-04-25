using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace RegimenCondominio.C
{
    public class SqlTransaction
    {        
        /// <summary>
        /// Definición de la transacción a ejecutar
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="input">The input.</param>
        /// <param name="bg">The bg.</param>
        public delegate object SQL_TransactionHandler(SQL_Connector conn, object input, BackgroundWorker bg);
        public delegate void SQL_TransactionFinishHandler(object input);
        Object Input;
        SQL_TransactionHandler Action;
        BackgroundWorker Bg;

        SQL_TransactionFinishHandler TaskIsFinish;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTransaction"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="taskAction">The action.</param>
        public SqlTransaction(Object input, SQL_TransactionHandler taskAction, SQL_TransactionFinishHandler taskIsFinished)
        {
            this.Input = input;
            this.Action=taskAction;
            this.TaskIsFinish = taskIsFinished;
            Bg = new BackgroundWorker();
            Bg.DoWork += Bg_DoWork;
            Bg.RunWorkerCompleted += Bg_RunWorkerCompleted;
        }
        /// <summary>
        /// Ejecuta la transaccion
        /// </summary>
        public void Run()
        {
            this.Bg.RunWorkerAsync();
        }

        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (TaskIsFinish != null)
                this.TaskIsFinish(e.Result);
        }

        public void SetProgressTask(ProgressChangedEventHandler reportTask)
        {
            Bg.ProgressChanged += reportTask;
            Bg.WorkerReportsProgress = true;
        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {            
            using (SQL_Connector conn = new SQL_Connector(Config.DB.ConnectionString))
            {
                try
                {
                    e.Result = this.Action(conn, this.Input, this.Bg);
                }
                catch (Exception exc)
                {
                    //MessageBox.Show(exc.ToString());
                    exc.Message.ToEditor();                    
                }
            }
        }
    }
}
