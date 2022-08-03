using System;
using System.Threading;

namespace MyCodeLibrary
{
    /// <summary>
    /// Мутекс именованный, для синхронизации двух потоков или проверки приложения на запущенность.
    /// </summary>
    ///<example>
    ///static NamedMutex m_Mutex;
    ///static string m_MutexName = "MutexName1";
    ///
    ///static void Main(string[] args)
    ///{
    ///    //check mutex exists
    ///    m_Mutex = new NamedMutex();
    ///    if (m_Mutex.Exists(m_MutexName) == true)
    ///    {
    ///        Console.WriteLine("Other copy of server already exists. Move to Exit.");
    ///        return;
    ///    }
    ///    //else
    ///    m_Mutex.Create(m_MutexName, true); 
    ///    ... тут работаем далее
    ///    m_Mutex.Release() - освобождаем мутекс для другого потока или в конце работы этого потока.
    ///    нельзя оставлять мутекс не освобожденным.
    ///    ... мутекс закрывается, когда срабатывает деструктор объекта ?
    ///    тут нет деструктора лучше явно вызвать Close(), но в этом классе этого нет.
    ///    Надо доделать класс!
    /// }
    /// </example>
    public class NamedMutex
    {
        /// <summary>
        /// Mutex internal object
        /// </summary>
        private Mutex m_Mutex;

        public NamedMutex()
        {
            m_Mutex = null;
        }

        ~NamedMutex()
        {
            if (m_Mutex != null)
            {
                m_Mutex.Close();
            }
        }
        /// <summary>
        /// NT-Create named mutex
        /// </summary>
        /// <param name="mutexName"></param>
        /// <param name="initiallyOwned"></param>
        public void Create(String mutexName, bool initiallyOwned)
        {
            m_Mutex = new Mutex(initiallyOwned, mutexName); 
        }

        /// <summary>
        /// NT-Проверить существование мутекса по имени и получить его если существует.
        /// </summary>
        /// <returns>Возвращается true если мутекс существует, иначе false </returns>
        public bool Exists(String mutexName)
        {
            bool result = false;
            try
            {
                m_Mutex = Mutex.OpenExisting(mutexName);
                result = true;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                result = false;
            }

            // There are three cases: (1) The mutex does not exist.
            // (2) The mutex exists, but the current user doesn't 
            // have access. (3) The mutex exists and the user has
            // access.
            //

            return result;
        }

        /// <summary>
        /// NT- Own mutex and wait for timeouts
        /// </summary>
        /// <param name="timeoutMs">
        /// The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely. 
        /// If timeout is zero, the method does not block. It tests the state of the mutex and returns immediately. 
        /// </param>
        /// <returns>Returns True if mutex owned. False if timeout occurs.</returns>
        public bool Own(int timeoutMs)
        {
            bool result = false;
            result = m_Mutex.WaitOne(timeoutMs, false);//TODO: что это значит?
            return result;
        }

        /// <summary>
        /// NT- Test mutex state
        /// </summary>
        /// <returns>Returns True if mutex owned. False if timeout occurs.</returns>
        public bool Test()
        {
            bool result = false;
            result = m_Mutex.WaitOne(0, false);//TODO: что это значит?
            return result;
        }
        /// <summary>
        /// NT-Release mutex
        /// </summary>
        public void Release()
        {
            m_Mutex.ReleaseMutex();
        }

    }
}
