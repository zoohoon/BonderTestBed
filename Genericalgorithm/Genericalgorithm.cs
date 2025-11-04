using LogModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ThreadState = System.Threading.ThreadState;

namespace Genericalgorithm
{
    public class GenericAlgorithm
    {
        #region GA Properties

        // Number Population
        int Npop = 500;

        // Number Keep Chromosome Size 
        int N_keep = 0;

        int countCPUCore = 1;
        int StartedTick = 0;

        CancellationTokenSource tokenSource;

        // Save DNA information in Chromosome Array
        Chromosome[] pop;

        // create new process or for end process
        //Thread RunTime;

        // save number of all city
        private int counter_City = 0;
        public int Counter_City { get { return counter_City; } } // ReadOnly

        // Double Array Pn for save Rank
        double[] Pn;

        public bool pGAToolStripMenuItem = false;
        public bool threadParallelismToolStripMenuItem = false;
        public bool taskParallelismToolStripMenuItem = false;
        public bool parallelForToolStripMenuItem = false;

        public int ToolStripProgressBar1Value;
        public int ToolStripProgressBar1MaximumValue;

        #endregion

        private void setCitiesPosition(List<Rect> ovalShape_City)
        {
            try
            {
                Chromosome.citiesPosition.Clear();
                foreach (var city in ovalShape_City)
                    Chromosome.citiesPosition.Add(city.Location);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private int calcCountOfCPU()
        {
            int numCore = 0;
            try
            {

                #region Find number of Active CPU or CPU core's for this Programs

                long Affinity_Dec = Process.GetCurrentProcess().ProcessorAffinity.ToInt64();
                string Affinity_Bin = Convert.ToString(Affinity_Dec, 2); // toBase 2
                foreach (char anyOne in Affinity_Bin.ToCharArray())
                    if (anyOne == '1') numCore++;

                #endregion

                //if (numCore > 2) return --numCore;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return numCore;
        }

        #region Generation Tools
        private void Population(Random rand)
        {
            try
            {
                // create first population by Npop = 500;
                pop = new Chromosome[Npop];

                int[] RandNum = new int[counter_City];

                int[] RandNumber = new int[counter_City];

                int buffer = counter_City - 1;

                for (int l = 0; l < counter_City; l++)
                    RandNumber[l] = l;
                for (int i = 0; i < Npop; i++)
                {
                    RandNum = RandNumber;
                    buffer = counter_City - 1;
                    pop[i] = new Chromosome(counter_City);
                    int b;
                    int buffer2;
                    for (int j = 0; j < counter_City; j++)
                    {
                        b = rand.Next(0, buffer);
                        pop[i].Tour[j] = RandNum[b];
                        buffer2 = RandNum[buffer];
                        RandNum[buffer] = RandNum[b];
                        RandNum[b] = buffer2;
                        buffer--;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //find percent of All chromosome rate for delete Amiss(xRate) or Useful(Nkeep) chromosome
        //x_Rate According by chromosome fitness Average 
        private void x_Rate()
        {
            try
            {
                // calculate Addition of all fitness
                double sumFitness = 0;
                for (int i = 0; i < Npop; i++)
                    sumFitness += pop[i].Fitness;
                // calculate Average of All chromosome fitness 
                double aveFitness = sumFitness / Npop; //Average of all chromosome fitness
                N_keep = 0; // N_keep start at 0 till Average fitness chromosome
                for (int i = 0; i < Npop; i++)
                    if (aveFitness >= pop[i].Fitness)
                    {
                        N_keep++; // counter as 0 ~ fitness Average + 1
                    }
                if (N_keep <= 0) N_keep = 2;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        // Definition Probability According by chromosome fitness 
        private void Rank_Trim()
        {
            try
            {
                // First Reserve Possibility Number for every Remnant chromosome 
                // chromosome Possibility Function is:
                // (1 + N_keep - No.chromosome) / ( ∑ No.chromosome) 
                // Where as at this program No.chromosome Of Array begin as Number 0
                // There for No.chromosome in Formula = No.chromosome + 1
                // then function is: if (n == N_keep)
                // Possibility[No.chromosome] = (n - No.chromosome) / (n(n+1) / 2)
                //
                Pn = new double[N_keep]; // Create chromosome possibility Array Cell as N_keep
                double Sum = ((N_keep * (N_keep + 1)) / 2); // (∑ No.chromosome) == (n(n+1) / 2)
                Pn[0] = N_keep / Sum; // Father (Best - Elite) chromosome Possibility
                for (int i = 1; i < N_keep; i++)
                {
                    // Example: if ( Pn[Elite] = 0.4  &  Pn[Elite +1] = 0.2  &  Pn[Elite +2]  = 0.1 )
                    // Then Own:          0 <= R <= 0.4 ===> Select chromosome[Elite]
                    //                  0.4 <  R <= 0.6 ===> Select chromosome[Elite +1] 
                    //                  0.6 <  R <= 0.7 ===> Select chromosome[Elite +2]
                    // etc ... 
                    Pn[i] = ((N_keep - i) / Sum) + Pn[i - 1];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        // Return Father and Mather chromosome with Probability of chromosome fitness
        private Chromosome Rank(Random rand)
        {
            try
            {
                double R = rand.NextDouble();
                for (int i = 0; i < N_keep; i++)
                {
                    // Example: if ( Pn[Elite] = 0.6  &  Pn[Elite+1] = 0.3  &  Pn[Elite+2]  = 0.1 )
                    // Then Own:          0 <= R <= 0.6  ===> Select chromosome[Elite]
                    //                  0.6 <  R <= 0.9  ===> Select chromosome[Elite +1] 
                    //                  0.9 <  R <= 1    ===> Select chromosome[Elite +2]
                    // 
                    if (R <= Pn[i]) return pop[i];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pop[0]; // if don't run Modality of 'for' then return Elite chromosome 
        }

        // Check the isotropy All REMNANT chromosome (N_keep)
        public bool Isotropy_Evaluatuon()
        {
            int per_Iso = Convert.ToInt32(Math.Truncate(Convert.ToDouble(50 * N_keep / 100)));
            int counter_Isotropy = 0;
            try
            {
                // Isotropy percent is 50% of All chromosome Fitness

                double BestFitness = pop[0].Fitness;
                //
                // i start at 1 because DNA_Array[0] is self BestFitness
                for (int i = 1; i < N_keep; i++)
                    if (BestFitness >= pop[i].Fitness) counter_Isotropy++;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            // G.A Algorithm did isotropy and app Stopped
            if (counter_Isotropy >= per_Iso) return false;
            else return true; // G.A Algorithm didn't isotropy and app will continued
        }

        public struct ThreadToken
        {
            public ThreadToken(int Thread_No, int _length, int _startIndex)
            {
                try
                {
                    No = Thread_No;
                    length = _length;
                    startIndex = _startIndex;
                    rand = new Random();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
            public int No;
            public int length;
            public int startIndex;
            public Random rand;
        };

        private void setThreadPriority(Thread th)
        {
            try
            {
                if (th != null)
                {
                    if (th.ThreadState != ThreadState.Aborted &&
                       th.ThreadState != ThreadState.AbortRequested &&
                       th.ThreadState != ThreadState.Stopped &&
                       th.ThreadState != ThreadState.StopRequested)
                    {
                        switch (Process.GetCurrentProcess().PriorityClass)
                        {
                            case ProcessPriorityClass.AboveNormal:
                                th.Priority = ThreadPriority.AboveNormal;
                                break;
                            case ProcessPriorityClass.BelowNormal:
                                th.Priority = ThreadPriority.BelowNormal;
                                break;
                            case ProcessPriorityClass.High:
                                th.Priority = ThreadPriority.Highest;
                                break;
                            case ProcessPriorityClass.Idle:
                                th.Priority = ThreadPriority.Lowest;
                                break;
                            case ProcessPriorityClass.Normal:
                                th.Priority = ThreadPriority.Normal;
                                break;
                            case ProcessPriorityClass.RealTime:
                                th.Priority = ThreadPriority.Highest;
                                break;
                        }
                        //
                        // Set Thread Affinity 
                        //
                        Thread.BeginThreadAffinity();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ReproduceByParallelThreads()
        {
            try
            {
                #region Parallel Reproduct Code
                Thread[] th = new Thread[countCPUCore];

                // Create a semaphore that can satisfy up to three
                // concurrent requests. Use an initial count of zero,
                // so that the entire semaphore count is initially
                // owned by the main program thread.
                //
                Semaphore sem = new Semaphore(countCPUCore, countCPUCore);
                bool[] isAlive = new bool[countCPUCore];
                bool[] isCompleted = new bool[countCPUCore];

                int length = (Npop - N_keep) / countCPUCore;
                int divideReminder = (Npop - N_keep) % countCPUCore;

                for (int proc = 0; proc < th.Length; proc++)
                {
                    ThreadToken tt = new ThreadToken(proc,
                        length + ((proc == countCPUCore - 1) ? divideReminder : 0),
                        N_keep + (proc * length));

                    th[proc] = new Thread(new ParameterizedThreadStart((x) =>
                    {
                    // Entered
                    sem.WaitOne();
                        isAlive[((ThreadToken)x).No] = true;

                    // work ...
                    PReproduction(((ThreadToken)x).startIndex, ((ThreadToken)x).length, ((ThreadToken)x).rand);

                    // We have finished our job, so release the semaphore
                    isCompleted[((ThreadToken)x).No] = true;
                        sem.Release();
                    }));
                    setThreadPriority(th[proc]);
                    th[proc].Start(tt);
                }

            startloop:
                foreach (bool alive in isAlive) // wait parent starter for start all children.
                    if (!alive)
                        goto startloop;

                    endLoop:
                sem.WaitOne();
                foreach (bool complete in isCompleted) // wait parent to interrupt for finishes all of children jobs.
                    if (!complete)
                        goto endLoop;

                // Continue Parent Work
                sem.Close();
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void ReproduceByParallelTask()
        {
            try
            {
                #region Parallel Reproduct Code
                Task[] tasks = new Task[countCPUCore];

                int length = (Npop - N_keep) / countCPUCore;
                int divideReminder = (Npop - N_keep) % countCPUCore;

                for (int proc = 0; proc < tasks.Length; proc++)
                {
                    ThreadToken tt = new ThreadToken(proc,
                        length + ((proc == countCPUCore - 1) ? divideReminder : 0),
                        N_keep + (proc * length));

                    tasks[proc] = Task.Factory.StartNew(x =>
                    {
                    // work ...
                    PReproduction(((ThreadToken)x).startIndex, ((ThreadToken)x).length, ((ThreadToken)x).rand);

                    }, tt, tokenSource.Token);// TaskCreationOptions.AttachedToParent);
                }

                // When user code that is running in a task creates a task with the AttachedToParent option, 
                // the new task is known as a child task of the originating task, 
                // which is known as the parent task. 
                // You can use the AttachedToParent option to express structured task parallelism,
                // because the parent task implicitly waits for all child tasks to complete. 
                // The following example shows a task that creates one child task:
                Task.WaitAll(tasks);

                // or

                //Block until all tasks complete.
                //Parent.Wait(); // when all task are into a parent task
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        /// <summary>
        /// Series Create New chromosome with Father & Mather Chromosome Instead of deleted chromosomes
        /// </summary>
        /// <param name="rand"></param>
        public void Reproduction(Random rand) // Series 
        {
            try
            {
                for (int i = N_keep; i < Npop; i++)
                {
                    //
                    // for send and check Father & Mather chromosome
                    Chromosome Rank_Father, Rank_Mather, child;

                    // have a problem (maybe Rank_1() == Rank_2()) then Father == Mather
                    // Solve Problem by Loop checker
                    do
                    {
                        Rank_Father = Rank(rand);
                        Rank_Mather = Rank(rand);
                    }
                    while (Rank_Father == Rank_Mather);
                    //
                    // Crossover
                    child = Rank_Father.crossover(Rank_Mather, rand);
                    //
                    //  run Mutation
                    //
                    child.mutation(rand);
                    //
                    // calculate children chromosome fitness
                    //
                    child.Calculate_Fitness();

                    Interlocked.Exchange(ref pop[i], child); // atomic operation between multiple Thread shared
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        /// <summary>
        /// Parallel Create New chromosome with Father & Mather Chromosome Instead of deleted chromosomes
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public void PReproduction(int startIndex, int length, Random rand) // Parallel 
        {
            try
            {
                for (int i = startIndex; i < (startIndex + length) && i < Npop; i++)
                {
                    //
                    // for send and check Father & Mather chromosome
                    Chromosome Rank_Father, Rank_Mather, child;

                    // have a problem (maybe Rank_1() == Rank_2()) then Father == Mather
                    // Solve Problem by Loop checker
                    do
                    {
                        Rank_Father = Rank(rand);
                        Rank_Mather = Rank(rand);
                    }
                    while (Rank_Father == Rank_Mather);
                    //
                    // Crossover
                    child = Rank_Father.crossover(Rank_Mather, rand);
                    //
                    //  run Mutation
                    //
                    child.mutation(rand);
                    //
                    // calculate children chromosome fitness
                    //
                    child.Calculate_Fitness();

                    Interlocked.Exchange(ref pop[i], child); // atomic operation between multiple Thread shared
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        /// <summary>
        /// Parallel Create New chromosome with Father & Mather Chromosome Instead of deleted chromosomes
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public void PReproduction(Random rand) // Parallel.For 
        {
            try
            {
                Parallel.For(N_keep, Npop,
                            new ParallelOptions() { MaxDegreeOfParallelism = countCPUCore, CancellationToken = tokenSource.Token },
                            (i, loopState) =>
                            {
                            // have a problem (maybe Rank_1() == Rank_2()) then Father == Mather
                            // Solve Problem by Loop checker
                            Chromosome Rank_Father, Rank_Mather, child;
                                do
                                {
                                    Monitor.Enter(rand);
                                    Rank_Father = Rank(rand);
                                    Rank_Mather = Rank(rand);
                                    Monitor.Exit(rand);
                                }
                                while (Rank_Father == Rank_Mather);
                            //
                            // Crossover
                            child = Rank_Father.crossover(Rank_Mather, rand);
                            //
                            //  run Mutation
                            //
                            child.mutation(rand);
                            //
                            // calculate children chromosome fitness
                            //
                            child.Calculate_Fitness();

                                Interlocked.Exchange(ref pop[i], child); // atomic operation between multiple Thread shared

                            if (tokenSource.IsCancellationRequested || tokenSource.Token.IsCancellationRequested)
                                {
                                    loopState.Stop();
                                    loopState.Break(); return;
                                }
                            });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        #endregion

        private void SetMaxValue(int v)
        {
            try
            {
                ToolStripProgressBar1MaximumValue = v;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //// InvokeRequired required compares the thread ID of the
            //// calling thread to the thread ID of the creating thread.
            //// If these threads are different, it returns true.
            //try
            //{
            //    if (this.statusStrip1.InvokeRequired)
            //    {
            //        SetMaxValueCallback d = new SetMaxValueCallback(SetMaxValue);
            //        this.Invoke(d, new object[] { v });
            //    }
            //    else
            //    {
            //        this.toolStripProgressBar1.Maximum = v;
            //    }
            //}
            //catch { }
        }

        public void SetValue(int v)
        {
            try
            {
                ToolStripProgressBar1Value = v;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //// InvokeRequired required compares the thread ID of the
            //// calling thread to the thread ID of the creating thread.
            //// If these threads are different, it returns true.
            //try
            //{
            //    if (this.statusStrip1.InvokeRequired)
            //    {
            //        SetValueCallback d = new SetValueCallback(SetValue);
            //        this.Invoke(d, new object[] { v });
            //    }
            //    else
            //    {
            //        this.toolStripProgressBar1.Value = v;
            //    }
            //}
            //catch { }
        }

        public int[] Compute(List<Rect> CityInfos)
        {
            try
            {
                int CompareCount = 10;
                double FitnessCompareAvg = 0;
                double FitnessCompareSum = 0;

                double DiffSum = 0;
                bool PassFlag = false;
                double PassFitnessTolerance = 10;

                List<Rect> ovalShape_City = new List<Rect>();

                foreach (var group in CityInfos)
                {
                    ovalShape_City.Add(group);
                }

                //
                // set cities position
                setCitiesPosition(ovalShape_City);

                //
                // initialize Parallel Computing for GA
                countCPUCore = calcCountOfCPU(); // Calculate number of active core or CPU for this app
                tokenSource = new CancellationTokenSource();

                //
                // set Start TickTime
                StartedTick = Environment.TickCount;

                //if (pGAToolStripMenuItem.Checked)  // clear Parallel points
                //{
                //    _pPlistTfg[1].Clear();
                //    _pPlistGfg[1].Clear();
                //    _pPlistTgg[1].Clear();
                //}
                //else // clear Series points
                //{
                //    _pPlistTfg[0].Clear();
                //    _pPlistGfg[0].Clear();
                //    _pPlistTgg[0].Clear();
                //}
                //
                Random rand = new Random();

                //double EliteFitness = double.MaxValue;

                #region Population

                counter_City = ovalShape_City.Count;
                // create first population by Npop = 500;
                Population(rand); // initialize population
                #endregion

                #region Evaluate Fitness
                for (int i = 0; i < Npop; i++)
                    pop[i].Calculate_Fitness();

                #endregion

                int count = 0;
                SetValue(0);
                //toolStripProgressBar1.Value = 0;
                //
                //SetGenerationText("0000");
                //lblGeneration.Text = "0000";
                //

                if (counter_City <= 5)
                    SetMaxValue(100);
                //toolStripProgressBar1.Maximum = 100;
                //
                else if (counter_City <= 15)
                    SetMaxValue(1000);
                //toolStripProgressBar1.Maximum = 1000;
                //
                else if (counter_City <= 30)
                    SetMaxValue(10000);
                //toolStripProgressBar1.Maximum = 10000;
                //
                else if (counter_City <= 40)
                    SetMaxValue(51000);
                //toolStripProgressBar1.Maximum = 51000;
                //
                else if (counter_City <= 60)
                    SetMaxValue(100000);
                //toolStripProgressBar1.Maximum = 100000;
                //
                else
                    SetMaxValue(1000000);
                //toolStripProgressBar1.Maximum = 1000000;
                //
                //

                do
                {
                    #region Selection
                    #region Bubble Sort all chromosome by fitness
                    // 
                    for (int i = Npop - 1; i > 0; i--)
                        for (int j = 1; j <= i; j++)
                            if (pop[j - 1].Fitness > pop[j].Fitness)
                            {
                                Chromosome ch = pop[j - 1];
                                pop[j - 1] = pop[j];
                                pop[j] = ch;
                            }
                    //
                    #endregion

                    //#region Elitism
                    //if (EliteFitness > pop[0].Fitness)
                    //{
                    //    EliteFitness = pop[0].Fitness;
                    //    setTimeGraph(EliteFitness, count, true);

                    //    if (dynamicalGraphicToolStripMenuItem.Checked) // Design if Graphically is ON
                    //    {
                    //        refreshTour();
                    //    }
                    //    //
                    //    //-----------------------------------------------------------------------------
                    //    SetLenghtText(pop[0].Fitness.ToString());
                    //    //
                    //}

                    //else setTimeGraph(EliteFitness, count, false); // just refresh Generation Graph's

                    #endregion
                    x_Rate(); // Selection any worst chromosome for clear and ...

                    #region Reproduction
                    // Definition Probability According by chromosome fitness 
                    // create Pn[N_keep];
                    Rank_Trim();

                    if (pGAToolStripMenuItem) // Parallel Genetic Algorithm
                    {
                        if (threadParallelismToolStripMenuItem) // PGA by MultiThreading
                        {
                            ReproduceByParallelThreads();
                        }
                        else if (taskParallelismToolStripMenuItem) // PGA by Task Parallelism
                        {
                            ReproduceByParallelTask();
                        }
                        else if (parallelForToolStripMenuItem) // PGA by Parallel.For ...
                        {
                            PReproduction(rand);
                        }
                    }
                    else // Series Genetic Algorithm
                    {
                        #region Series Reproduct Code
                        Reproduction(rand);
                        #endregion
                    }
                    #endregion

                    count++;
                    //SetGenerationText(count.ToString());
                    //lblGeneration.Text = count.ToString();
                    //
                    //SetValue(toolStripProgressBar1.Value + 1);
                    //toolStripProgressBar1.Value++;
                    //

                    FitnessCompareSum = 0;

                    for (int i = 0; i < CompareCount; i++)
                    {
                        FitnessCompareSum += pop[i].Fitness;
                    }

                    FitnessCompareAvg = FitnessCompareSum / CompareCount;

                    PassFlag = false;

                    for (int i = 0; i < CompareCount; i++)
                    {
                        DiffSum = Math.Abs(pop[i].Fitness - FitnessCompareAvg);

                        if (DiffSum > PassFitnessTolerance)
                        {
                            PassFlag = false;
                            break;
                        }
                        else
                        {
                            PassFlag = true;
                        }
                    }

                    if (PassFlag == true)
                    {
                        break;
                    }
                }
                while (count < ToolStripProgressBar1MaximumValue && Isotropy_Evaluatuon());

                //
                //toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
                //SetValue(ToolStripProgressBar1MaximumValue);

                //
                // UnLock numUpDownPop
                //setNumPopEnable(true);
                //
                // The END
                //Stop();

                //double SumDistance = 0;

                //for (int i = 0; i < pop[0].Tour.Length - 1; i++)

                //{
                //    double distance;

                //    Point p1, p2;

                //    int groupIndex_1 = pop[0].Tour[i];
                //    int groupIndex_2 = pop[0].Tour[i + 1];

                //    double left, top;

                //    left = PMIPadGroups[groupIndex_1].GroupPosition.Left + (PMIPadGroups[groupIndex_1].GroupPosition.Width / 2);
                //    top = PMIPadGroups[groupIndex_1].GroupPosition.Top + (PMIPadGroups[groupIndex_1].GroupPosition.Height / 2);

                //    p1 = new Point(left, top);

                //    left = PMIPadGroups[groupIndex_2].GroupPosition.Left + (PMIPadGroups[groupIndex_2].GroupPosition.Width / 2);
                //    top = PMIPadGroups[groupIndex_2].GroupPosition.Top + (PMIPadGroups[groupIndex_2].GroupPosition.Height / 2);

                //    p2 = new Point(left, top);

                //    distance = Point.Subtract(p2, p1).Length;

                //    SumDistance += distance;
                //}
                //foreach (var city in pop[0].Tour)
                //{
                //    double distance;

                //    Point p1, p2;

                //    p1 = new Point();

                //    distance = Point.Subtract(p2, p1).Length;

                //    SumDistance += 

                //    ovalShape_City.Add(group.GroupPosition);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }


            return pop[0].Tour;
        }

    }
}
