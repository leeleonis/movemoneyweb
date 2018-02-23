using BitcoinTransactionManagement.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace BitcoinTransactionManagement
{
    public class SetExecutions
    {
        public static void StartProcess(string ProcessName, string ExecutionsId, string Url)
        {
            try
            {
                var Application = HttpContext.Current.Application;
                List<ProcessModel> ExecutionsProcessList = (List<ProcessModel>)Application["ExecutionsProcess"] ?? new List<ProcessModel>();
                var ExecutionsProcess = ExecutionsProcessList.Find(x => x.ExecutionsId == ExecutionsId);
                //如果Application["ExecutionsProcess"]找不到此ExecutionsId
                if (ExecutionsProcess == null)
                {
                    ExecutionsProcessList.Add(new ProcessModel { Process = Process.Start(ProcessName, ExecutionsId + " " + Url), ExecutionsId = ExecutionsId, Status = 1, Createdt = DateTime.Now });
                    Application.Set("ExecutionsProcess", ExecutionsProcessList);
                }
                else
                {
                    try
                    {
                        Process.GetProcessById(ExecutionsProcess.Process.Id);
                    }
                    catch (Exception)
                    {
                        ExecutionsProcess.Process.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void KillProcess(string ExecutionsId)
        {
            try
            {
                var Application = HttpContext.Current.Application;
                List<ProcessModel> ExecutionsProcessList = (List<ProcessModel>)Application["ExecutionsProcess"] ?? new List<ProcessModel>();
                //如果Application["ExecutionsProcess"]有此ExecutionsId
                var ExecutionsProcess = ExecutionsProcessList.Find(x => x.ExecutionsId == ExecutionsId);
                if (ExecutionsProcess != null)
                {
                    ExecutionsProcess.Process.Kill();
                    ExecutionsProcessList.Remove(ExecutionsProcess);
                    Application.Set("ExecutionsProcess", ExecutionsProcessList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void ShouldStartProcess(string ProcessName, List<Executions> ExecutionsList, string Url)
        {
            try
            {
                var Application = HttpContext.Current.Application;
                List<ProcessModel> ExecutionsProcessList = (List<ProcessModel>)Application["ExecutionsProcess"] ?? new List<ProcessModel>();
                foreach (var Executions in ExecutionsList)
                {
                    var ExecutionsId = Executions.id.ToString();
                    var ExecutionsProcess = ExecutionsProcessList.Find(x => x.ExecutionsId == ExecutionsId);
                    if (Executions.Status==1)
                    {
                        if (ExecutionsProcess == null)
                        {
                            StartProcess(ProcessName, ExecutionsId, Url);
                        }
                        else if (ExecutionsProcess.Status==1)
                        {
                            try
                            {
                                Process.GetProcessById(ExecutionsProcess.Process.Id);
                            }
                            catch (Exception)
                            {
                                ExecutionsProcess.Process.Start();
                            }
                        }
                    }
                    else if (Executions.Status == 0)
                    {
                        if (ExecutionsProcess != null)
                        {
                            ExecutionsProcessList.Remove(ExecutionsProcess);
                            Application.Set("ExecutionsProcess", ExecutionsProcessList);
                            var ThisProcess = Process.GetProcessById(ExecutionsProcess.Process.Id);
                            if (ThisProcess != null)
                            {
                                ThisProcess.Kill();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void ShouldKillProcess(string ExecutionsId, int ProcessId)
        {
            try
            {
                var Application = HttpContext.Current.Application;
                List<ProcessModel> ExecutionsProcessList = (List<ProcessModel>)Application["ExecutionsProcess"] ?? new List<ProcessModel>();
                //如果Application["ExecutionsProcess"]無此ExecutionsId 或狀態是關
                var ExecutionsProcess = ExecutionsProcessList.Find(x => x.ExecutionsId == ExecutionsId);
                if (ExecutionsProcess == null || ExecutionsProcess.Status != 1)
                {
                    var ThisProcess = Process.GetProcessById(ProcessId);
                    if (ThisProcess != null)
                    {
                        ThisProcess.Kill();
                    }
                    if (ExecutionsProcess != null)
                    {
                        ExecutionsProcessList.Remove(ExecutionsProcess);
                        Application.Set("ExecutionsProcess", ExecutionsProcessList);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }

    public class ProcessModel
    {
        public Process Process { get; set; }
        public string ExecutionsId { get; set; }
        public int Status { get; set; }
        public DateTime Createdt { get; set; }
    }

}