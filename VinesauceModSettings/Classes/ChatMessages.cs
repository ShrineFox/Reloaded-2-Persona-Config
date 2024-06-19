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
                    Directory.CreateDirectory(Path.GetDirectoryName(msgPath));
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

        private void UpdateChatPingSFX(string modDir, bool usePingSFX)
        {
            string awbEmuDir01 = Path.Combine(modDir, "FEmulator\\AWB\\SPT01.AWB");
            string awbEmuDir02 = Path.Combine(modDir, "FEmulator\\AWB\\SPT02.AWB");
            string pingSfxDir = Path.Combine(modDir, "Mod Files\\Toggleable\\Chat Navi\\SFX\\Ping");
            string silentSfxDir = Path.Combine(modDir, "Mod Files\\Toggleable\\Chat Navi\\SFX\\Silent");

            // Add silent navi SFX to FEmu if it's missing
            if (!Directory.Exists(awbEmuDir01) && Directory.Exists(silentSfxDir))
            {
                CopyDir(silentSfxDir, Path.GetDirectoryName(awbEmuDir01));
                _logger.WriteLine($"Created silent navi SFX.", System.Drawing.Color.Green);
            }
            if (!Directory.Exists(awbEmuDir02) && Directory.Exists(silentSfxDir))
            {
                CopyDir(silentSfxDir, Path.GetDirectoryName(awbEmuDir02));
                _logger.WriteLine($"Created silent navi SFX.", System.Drawing.Color.Green);
            }

            // Replace SFX based on user's mod settings
            if (Directory.Exists(awbEmuDir01) && Directory.Exists(awbEmuDir02)
                && Directory.Exists(pingSfxDir) && Directory.Exists(silentSfxDir))
            {
                if (usePingSFX && !Directory.GetFiles(awbEmuDir01, "*.adx", SearchOption.AllDirectories).Any(x => x.Contains("_ping")))
                {
                    Directory.Delete(awbEmuDir01, true);
                    Directory.Delete(awbEmuDir02, true);
                    CopyDir(pingSfxDir, Path.GetDirectoryName(awbEmuDir01));
                    CopyDir(pingSfxDir, Path.GetDirectoryName(awbEmuDir02));
                    _logger.WriteLine($"Copied ping SFX to navi chat directory.", System.Drawing.Color.Green);
                }
                else if (!usePingSFX && !Directory.GetFiles(awbEmuDir01, "*.adx", SearchOption.AllDirectories).Any(x => x.Contains("_silence")))
                {
                    Directory.Delete(awbEmuDir01, true);
                    Directory.Delete(awbEmuDir02, true);
                    CopyDir(silentSfxDir, Path.GetDirectoryName(awbEmuDir01));
                    CopyDir(silentSfxDir, Path.GetDirectoryName(awbEmuDir02));
                    _logger.WriteLine($"Copied silent SFX to navi chat directory.", System.Drawing.Color.Green);
                }
                else
                    _logger.WriteLine($"Skipping chat navi SFX update...");
            }
            else
                _logger.WriteLine($"Failed to update chat navi SFX, a required directory was missing.", System.Drawing.Color.Red);
        }

        public static void CopyDir(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder) && !File.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            // Get Files & Copy
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
            }

            // Get dirs recursively and copy files
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyDir(folder, dest);
            }
        }
    }
}
