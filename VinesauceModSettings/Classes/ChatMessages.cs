using Reloaded.Mod.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinesauceModSettings.Template;

namespace VinesauceModSettings
{
    public partial class Mod : ModBase // <= Do not Remove.
    {
        private void RewriteChatMessages(string txtPath)
        {
            if (File.Exists(txtPath))
            {
                var chatLines = File.ReadAllLines(txtPath);
                string txtDir = Path.GetDirectoryName(txtPath);

                List<int> navi_IDs = new List<int>() { 3, 8, 9 };
                foreach (int navi_id in navi_IDs)
                {
                    string msgPath = Path.Combine(txtDir, "BMD", $"NAVI_0{navi_id}.msg");
                    string msgTxt = "";
                    for (int i = 0; i < 1282; i++)
                    {
                        Random random = new Random();
                        string newLine = "";
                        int voiceClipIndex = 1;
                        newLine += $"\n[msg btl_support_0{navi_id}_{i + 1}]\n";
                        if (_configuration.MimicTwitchChat)
                        {
                            for (int x = 0; x < 4; x++)
                            {
                                //if (x == 3)
                                    //voiceClipIndex = 2;
                                newLine += $"[s][vp 8 1 65535 {voiceClipIndex} 0 0] {chatLines[random.Next(0, chatLines.Length - 1)]}[n][f 1 6 65534][e]\n";
                            }
                        }
                        else
                            newLine += $"[s][vp 8 1 65535 2 0 0]{chatLines[random.Next(0, chatLines.Length - 1)]}[n][f 1 6 65534][e]\n";

                        msgTxt += newLine;
                    }
                    File.WriteAllText(msgPath, msgTxt);

                    _logger.WriteLine($"Finished randomizing lines in:\n\"{msgPath}\"", System.Drawing.Color.Green);
                }
            }
            else
                _logger.WriteLine($"Failed to randomize chat messages, could not locate file:\n\"{Path.GetFullPath(txtPath)}\"", System.Drawing.Color.Red);
        }

        private void CompileNaviMSGsToBMD(string txtDir)
        {
            // NOTE: Obsolete due to BMD Merging

            string compilerPathTxt = Path.Combine(txtDir, "AtlusScriptCompilerPath.txt");
            if (File.Exists(compilerPathTxt))
            {
                string compilerPath = File.ReadAllText(compilerPathTxt);
                if (!File.Exists(compilerPath))
                {
                    _logger.WriteLine($"Failed to recompile chat messages, could not locate file:\n\"{Path.GetFullPath(compilerPath)}\"", System.Drawing.Color.Red);
                    return;
                }

                foreach (var msgPath in Directory.GetFiles(txtDir, "*.msg", SearchOption.TopDirectoryOnly))
                {
                    string outBmd = msgPath.Replace(".msg", "");
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo(compilerPath);
                    p.StartInfo.FileName = compilerPath;
                    p.StartInfo.Arguments =
                        $"\"{msgPath}\" -Compile -Library P5R -Encoding P5R_EFIGS -OutFormat V1BE -Out \"{outBmd}\"";
                    p.Start();
                    p.WaitForExit();
                    _logger.WriteLine($"Recompiled chat messages to navigator .BMD:\n\"{Path.GetFullPath(outBmd)}\"", System.Drawing.Color.Green);

                }
            }
            else
                _logger.WriteLine($"Failed to recompile chat messages, could not locate file:\n\"{Path.GetFullPath(compilerPathTxt)}\"", System.Drawing.Color.Red);

        }
    }
}
