using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;

namespace DataCatGen_Console
{
    internal class Program
    {
        private static Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        private readonly static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static string register_sitekey = "4c672d35-0701-42b2-88c3-78380b0db560";
        private static string phone_sitekey = "f5561ba9-8f1e-40ca-9b5b-a0b3f719ef34";
        private static string anti_key = "59dff447088ad6fbaaea76a78f99de64";

        private static string fivesim_key =
            "eyJhbGciOiJSUzUxMiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE2NzcxMjkzODksImlhdCI6MTY0NTU5MzM4OSwicmF5IjoiOTE1YTAwZTRlZDMwZDYyOWM1MTc1NTg5NDVjOGFmN2MiLCJzdWIiOjk1MDUzNH0.AzbSJvJCX9kwmlDXn8camVuJN37XNsPr1wCLI3zq3nMsazVx2QnWtJjH3yDUAPznoxAVMfErQ-uf87tZmtiKOnaHVsn4zm-ZdF3xWYgOiaANk4Z3gZctbiI4YqsLADD1Z37eBONzvRuXhUyHRJD-VicW68allF4c1m_L7mlvFutnlvmeu5EW5_5dslFQC5GjQ5-eTAe5QXjwZMN1LLyWN-Q-mfZlYhBIhbj1gTyYkgsPjOMjfEeDyHnwV3aGS9o0N3xCusTWXeFpKW-sGwFVIqO7ukk1w9UIsvObH7e2PVOuArZCh9LdAtEK9nM9UD-Hv2Yx6wFz1h3gyRYNKGBYRA";

        private static Page discord_page;
        private static Page mail_page;

        private static string current_dir = AppDomain.CurrentDomain.BaseDirectory;
        static async Task Main(string[] args)
        {
            if (!File.Exists(current_dir + "tokens.txt"))
            {
                File.Create(current_dir + "tokens.txt");
            }

            var extra = new PuppeteerExtra();
            extra.Use(new StealthPlugin());

            Browser browser = await extra.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                ExecutablePath =
                    @"C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\browser\chrome.exe",
                Args = new string[]
                {
                    @"-disable-extensions-except=C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\anti,C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\block",
                    @"--load-extension=C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\anti,C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\block"
                },
                DefaultViewport = null
            });

            Thread.Sleep(5000);
            
            Page system_page = await browser.NewPageAsync();
            await system_page.BringToFrontAsync();
            await system_page.GoToAsync("chrome://system");
            await system_page.WaitForSelectorAsync("#extensions-value-btn");
            await system_page.ClickAsync("#extensions-value-btn");
            string extensions_id =
                await system_page.EvaluateFunctionAsync<string>("()=>document.querySelector('#extensions-value').textContent");
            string[] id_list = extensions_id.Split(new string[] { "\n" }, StringSplitOptions.None);
            string target_id = "";
            foreach (string id in id_list)
            {
                if (id.Contains("AntiCaptcha"))
                {
                    string[] id_split = id.Split(new string[] { ":" }, StringSplitOptions.None);
                    target_id = id_split[0].Replace(" ", "");
                }
            }

            await system_page.CloseAsync();
            
            string email = "";

            try
            {
                Page extension_page = await browser.NewPageAsync();
                await extension_page.SetBypassCSPAsync(true);
                await extension_page.GoToAsync(String.Format("chrome-extension://{0}/options.html", target_id));

                await extension_page.ClickAsync("#auto_submit_form");
                await extension_page.ClickAsync("#delay_onready_callback");
                await extension_page.TypeAsync("body > div > div.options_form > input[type=text]:nth-child(7)",
                    "59dff447088ad6fbaaea76a78f99de64");
                await extension_page.ClickAsync("body > div > input");

                await extension_page.CloseAsync();

                mail_page = await browser.NewPageAsync();
                mail_page.GoToAsync("https://temp-mail.org/en/");

                await mail_page.WaitForSelectorAsync("#mail", new WaitForSelectorOptions() { Timeout = 10 * 1000 });

                Thread.Sleep(2000);
                while (true)
                {
                    email = await mail_page.EvaluateFunctionAsync<string>("()=>document.querySelector('#mail').value");
                    if (email.Contains("Loading"))
                    {
                        Console.WriteLine(email);
                        Thread.Sleep(5000);
                    }
                    else if (String.IsNullOrEmpty(email))
                    {
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (PuppeteerException ex)
            {
                Console.WriteLine("Init Thread Error");
            }

            string username = RandomString(8);
            string password = RandomString(8) + "321!@#";
            string year = "2000";
            string month = "5";
            string day = "23";

            File.AppendAllText(current_dir + "tokens.txt", String.Format("{0}:{1}{2}", email, password, Environment.NewLine));

            Console.WriteLine("========INFO========");
            Console.WriteLine("email: {0}", email);
            Console.WriteLine("username: {0}", username);
            Console.WriteLine("password: {0}", password);
            Console.WriteLine("dob: {0}", String.Format("{0}-{1}-{2}", year, month, day));

            try
            {
                discord_page = await browser.NewPageAsync();
                await discord_page.BringToFrontAsync();
                await discord_page.GoToAsync("https://discord.com/register");
                //await discord_page.TypeAsync("input[name=email]", email);
                HumanizeInput(discord_page, "input[name=email]", email);
                Thread.Sleep(random.Next(1500, 3900));
                //await discord_page.TypeAsync("input[name=username]", username);
                HumanizeInput(discord_page, "input[name=username]", username);
                Thread.Sleep(random.Next(1500, 3900));
                //await discord_page.TypeAsync("input[name=password]", password);
                HumanizeInput(discord_page, "input[name=password]", password);
                Thread.Sleep(random.Next(1500, 3900));
                //await discord_page.ClickAsync("input[name=password]");
                Thread.Sleep(random.Next(500, 1200));
                await discord_page.Keyboard.PressAsync("Tab");
                //await discord_page.Keyboard.TypeAsync(month);
                HumanizeRawInput(discord_page, month);
                Thread.Sleep(random.Next(1500, 3900));
                await discord_page.Keyboard.PressAsync("Tab");
                //await discord_page.Keyboard.TypeAsync(day);
                HumanizeRawInput(discord_page, day);
                Thread.Sleep(random.Next(1500, 3900));
                await discord_page.Keyboard.PressAsync("Tab");
                //await discord_page.Keyboard.TypeAsync(year);
                HumanizeRawInput(discord_page, year);
                Thread.Sleep(random.Next(3000, 5900));
                await discord_page.ClickAsync(
                    "#app-mount > div.app-3xd6d0 > div > div > div > form > div > div > div:nth-child(5) > button");
                Thread.Sleep(5000);
                await discord_page.Keyboard.PressAsync("Tab");
                Thread.Sleep(500);
                await discord_page.Keyboard.PressAsync("Enter");
            }
            catch (PuppeteerException ex)
            {
                Console.WriteLine("Discord Register Error");
            }

            try
            {
                await mail_page.BringToFrontAsync();
                string inbox_link = "";
                //Thread.Sleep(10000);
                while (true)
                {
                    try
                    {
                        inbox_link = await mail_page.EvaluateFunctionAsync<string>(
                            "()=>document.querySelector('#tm-body > main > div.container > div > div.col-sm-12.col-md-12.col-lg-12.col-xl-8 > div.tm-content > div > div.inboxWarpMain > div > div.inbox-dataList > ul > li:nth-child(2) > div:nth-child(3) > div.m-link-view > a').getAttribute('href')");
                        Thread.Sleep(1000);
                        Console.WriteLine(inbox_link);
                        break;
                    }
                    catch (EvaluationFailedException ex)
                    {
                        Thread.Sleep(10000);
                    }
                }

                await mail_page.GoToAsync(inbox_link);
                Thread.Sleep(3000);
                string mail_context = "";
                while (true)
                {
                    try
                    {
                        mail_context = await mail_page.EvaluateFunctionAsync<string>(
                            "()=>document.querySelector('#tm-body > main > div.container > div > div.col-sm-12.col-md-12.col-lg-12.col-xl-8 > div.tm-content > div > div.inboxWarpMain > div > div.inbox-data-content > div.inbox-data-content-intro > div > div:nth-child(2) > div > table > tbody > tr > td > div > table > tbody > tr:nth-child(2) > td > table > tbody > tr > td > a').href");
                        Thread.Sleep(1000);
                        Console.WriteLine(inbox_link);
                        break;
                    }
                    catch (EvaluationFailedException ex)
                    {
                        Thread.Sleep(10000);
                    }
                }

                Console.WriteLine(mail_context);

                await mail_page.GoToAsync(mail_context);
                Thread.Sleep(5000);
                await discord_page.Keyboard.PressAsync("Tab");
                Thread.Sleep(500);
                await discord_page.Keyboard.PressAsync("Enter");
                await mail_page.WaitForSelectorAsync(
                    "#app-mount > div.app-3xd6d0 > div > div > div > section > div > button",
                    new WaitForSelectorOptions() { Visible = true, Timeout = 300 * 1000 });
                await mail_page.ClickAsync("#app-mount > div.app-3xd6d0 > div > div > div > section > div > button");
            }
            catch (PuppeteerException ex)
            {
                Console.WriteLine("Email Verfication Error");
            }

            Thread.Sleep(60000);

            await browser.CloseAsync();

            browser = await extra.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                ExecutablePath =
                    @"C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\browser\chrome.exe",
                Args = new string[]
                {
                    @"-disable-extensions-except=C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\anti,C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\block",
                    @"--load-extension=C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\anti,C:\Users\LUNAFE\Desktop\GitHub\DataCatGen-Console\block"
                },
                DefaultViewport = null
            });

            Page extension_page2 = await browser.NewPageAsync();
            await extension_page2.SetBypassCSPAsync(true);
            await extension_page2.GoToAsync("chrome-extension://jigojiooafbehjolkkblolhignldakdm/options.html");

            await extension_page2.ClickAsync("#auto_submit_form");
            await extension_page2.ClickAsync("#delay_onready_callback");
            await extension_page2.TypeAsync("body > div > div.options_form > input[type=text]:nth-child(7)",
                "59dff447088ad6fbaaea76a78f99de64");
            await extension_page2.ClickAsync("body > div > input");

            await extension_page2.CloseAsync();

            Page discord_page2 = await browser.NewPageAsync();
            await discord_page2.GoToAsync("https://discord.com/login");
            await discord_page2.BringToFrontAsync();

            HumanizeInput(discord_page2,
                "#app-mount > div.app-3xd6d0 > div > div > div > div > form > div > div > div.mainLoginContainer-wHmAjP > div.block-3uVSn4.marginTop20-2T8ZJx > div.marginBottom20-315RVT > div > div.inputWrapper-1YNMmM.inputWrapper-3ESIDR > input",
                email);
            Thread.Sleep(random.Next(1501, 3201));
            HumanizeInput(discord_page2,
                "#app-mount > div.app-3xd6d0 > div > div > div > div > form > div > div > div.mainLoginContainer-wHmAjP > div.block-3uVSn4.marginTop20-2T8ZJx > div:nth-child(2) > div > input",
                password);
            Thread.Sleep(random.Next(1501, 3201));
            await discord_page2.ClickAsync(
                "#app-mount > div.app-3xd6d0 > div > div > div > div > form > div > div > div.mainLoginContainer-wHmAjP > div.block-3uVSn4.marginTop20-2T8ZJx > button.marginBottom8-emkd0_.button-1cRKG6.button-f2h6uQ.lookFilled-yCfaCM.colorBrand-I6CyqQ.sizeLarge-3mScP9.fullWidth-fJIsjq.grow-2sR_-F");


            try
            {
                await discord_page2.WaitForSelectorAsync(
                    "#app-mount > div:nth-child(7) > div > div > div.flex-2S1XBF.flex-3BkGQD.vertical-3aLnqW.flex-3BkGQD.directionColumn-3pi1nm.justifyCenter-rrurWZ.alignCenter-14kD11.noWrap-hBpHBz.container-1Yj1aL > div.flex-2S1XBF.flex-3BkGQD.horizontal-112GEH.horizontal-1Piu5-.flex-3BkGQD.directionRow-2Iu2A9.justifyCenter-rrurWZ.alignStretch-Uwowzr.noWrap-hBpHBz > button",
                    new WaitForSelectorOptions() { Timeout = 300 * 1000, Visible = true });

                /*await discord_page2.ClickAsync(
                    "#app-mount > div:nth-child(7) > div > div > div.flex-2S1XBF.flex-3BkGQD.vertical-3aLnqW.flex-3BkGQD.directionColumn-3pi1nm.justifyCenter-rrurWZ.alignCenter-14kD11.noWrap-hBpHBz.container-1Yj1aL > div.flex-2S1XBF.flex-3BkGQD.horizontal-112GEH.horizontal-1Piu5-.flex-3BkGQD.directionRow-2Iu2A9.justifyCenter-rrurWZ.alignStretch-Uwowzr.noWrap-hBpHBz > button");
    */
                await discord_page2.EvaluateFunctionAsync<Object>(
                    "()=>document.getElementsByClassName('colorBrand-I6CyqQ')[4].click()");

                Thread.Sleep(5500);

                await discord_page2.ClickAsync(
                    "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > div.flex-2S1XBF.flex-3BkGQD.horizontal-112GEH.horizontal-1Piu5-.flex-3BkGQD.directionRow-2Iu2A9.justifyStart-2Mwniq.alignCenter-14kD11.noWrap-hBpHBz.phoneField-3NAPDv.elevationLow-26BbEG.field-3rN-Ip > button.countryButton-1cNDvB.button-f2h6uQ.lookFilled-yCfaCM.colorGrey-2iAG-B.sizeSmall-wU2dO-.grow-2sR_-F");

                //await discord_page2.Keyboard.TypeAsync("russia");
                HumanizeRawInput(discord_page2, "russia");

                Thread.Sleep(random.Next(1054, 1901));

                await discord_page2.ClickAsync(
                    "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > div.flex-2S1XBF.flex-3BkGQD.horizontal-112GEH.horizontal-1Piu5-.flex-3BkGQD.directionRow-2Iu2A9.justifyStart-2Mwniq.alignCenter-14kD11.noWrap-hBpHBz.phoneField-3NAPDv.elevationLow-26BbEG.field-3rN-Ip > div > div.phoneFieldScroller-2DblLb.auto-2K3UW5.scrollerBase-_bVAAt > div.flex-2S1XBF.flex-3BkGQD.horizontal-112GEH.horizontal-1Piu5-.flex-3BkGQD.directionRow-2Iu2A9.justifyStart-2Mwniq.alignCenter-14kD11.noWrap-hBpHBz.selectableItem-3-fmiM");

                Object[] phone_info = FiveSimCreatePhone();

                Thread.Sleep(random.Next(1554, 2901));

                /*await discord_page2.TypeAsync(
                    "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > div.flex-2S1XBF.flex-3BkGQD.horizontal-112GEH.horizontal-1Piu5-.flex-3BkGQD.directionRow-2Iu2A9.justifyStart-2Mwniq.alignCenter-14kD11.noWrap-hBpHBz.phoneField-3NAPDv.elevationLow-26BbEG.field-3rN-Ip > input",
                    phone_info[1].ToString().Replace("+7", ""));*/
                HumanizeInput(discord_page2,
                    "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > div.flex-2S1XBF.flex-3BkGQD.horizontal-112GEH.horizontal-1Piu5-.flex-3BkGQD.directionRow-2Iu2A9.justifyStart-2Mwniq.alignCenter-14kD11.noWrap-hBpHBz.phoneField-3NAPDv.elevationLow-26BbEG.field-3rN-Ip > input",
                    phone_info[1].ToString().Replace("+7", ""));

                Thread.Sleep(random.Next(2054, 3901));

                await discord_page2.ClickAsync(
                    "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > div.flex-2S1XBF.flex-3BkGQD.horizontal-112GEH.horizontal-1Piu5-.flex-3BkGQD.directionRow-2Iu2A9.justifyStart-2Mwniq.alignCenter-14kD11.noWrap-hBpHBz.phoneField-3NAPDv.elevationLow-26BbEG.field-3rN-Ip > button.sendButton-3xHNhl.button-f2h6uQ.lookFilled-yCfaCM.colorBrand-I6CyqQ.sizeSmall-wU2dO-.grow-2sR_-F");

                Thread.Sleep(5000);
                await discord_page.Keyboard.PressAsync("Tab");
                Thread.Sleep(500);
                await discord_page.Keyboard.PressAsync("Enter");

                await discord_page2.WaitForSelectorAsync(
                    "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > div.flex-2S1XBF.flex-3BkGQD.vertical-3aLnqW.flex-3BkGQD.directionColumn-3pi1nm.justifyStart-2Mwniq.alignCenter-14kD11.noWrap-hBpHBz.field-3rN-Ip > div > input:nth-child(1)",
                    new WaitForSelectorOptions() { Timeout = 300 * 1000, Visible = true });

                //await discord_page2.ClickAsync(
                //   "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > div.flex-2S1XBF.flex-3BkGQD.vertical-3aLnqW.flex-3BkGQD.directionColumn-3pi1nm.justifyStart-2Mwniq.alignCenter-14kD11.noWrap-hBpHBz.field-3rN-Ip > div > input:nth-child(1)");

                string code = "";
                while (true)
                {
                    code = FiveSimGetPhone(phone_info[0].ToString());
                    if (code != null)
                    {
                        Console.WriteLine(code);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Wating Verify Code...");
                        Thread.Sleep(10000);
                    }
                }

                Thread.Sleep(4000);

                foreach (char c in code)
                {
                    await discord_page2.Keyboard.TypeAsync(Char.ToString(c));
                    Thread.Sleep(random.Next(104, 460));
                }

                await discord_page2.WaitForSelectorAsync(
                    "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > form > div.content-2hZxGK.thin-31rlnD.scrollerBase-_bVAAt > div.spacing-2Z1zjR.marginBottom20-315RVT > div > input",
                    new WaitForSelectorOptions() { Timeout = 15 * 1000, Visible = true });

                Thread.Sleep(random.Next(1554, 2901));

                await discord_page2.TypeAsync(
                    "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > form > div.content-2hZxGK.thin-31rlnD.scrollerBase-_bVAAt > div.spacing-2Z1zjR.marginBottom20-315RVT > div > input",
                    "sdf");

                Thread.Sleep(random.Next(1954, 3901));

                await discord_page2.ClickAsync(
                    "#app-mount > div:nth-child(7) > div.layer-1Ixpg3 > div > div > form > div.flex-2S1XBF.flex-3BkGQD.horizontalReverse-60Katr.horizontalReverse-2QssvL.flex-3BkGQD.directionRowReverse-HZatnx.justifyStart-2Mwniq.alignStretch-Uwowzr.noWrap-hBpHBz.footer-31IekZ > button.button-f2h6uQ.lookFilled-yCfaCM.colorBrand-I6CyqQ.sizeMedium-2bFIHr.grow-2sR_-F");
            }
            catch (PuppeteerException ex)
            {
                Console.WriteLine("Phone Verification Error");
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }
        private static void HumanizeInput(Page page, string selector, string input)
        {
            foreach (char c in input)
            {
                page.TypeAsync(selector, Char.ToString(c));
                Thread.Sleep(random.Next(104, 460));
            }
        }

        private static void HumanizeRawInput(Page page, string input)
        {
            foreach (char c in input)
            {
                page.Keyboard.TypeAsync(Char.ToString(c));
                Thread.Sleep(random.Next(104, 460));
            }
        }

        private static object[] InitCFCookie()
        {
            HttpWebRequest web_request =
                (HttpWebRequest)WebRequest.Create("https://discord.com/register");
            web_request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            web_request.Method = "GET";
            web_request.Host = "discord.com";
            web_request.Headers.Add("Sec-Ch-Ua", "\"(Not(A:Brand\";v=\"8\", \"Chromium\";v=\"98\"");
            web_request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            web_request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36";
            web_request.ContentType = "application/json";
            web_request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            web_request.Accept =
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            web_request.Headers.Add("Sec-Fetch-Site", "none");
            web_request.Headers.Add("Sec-Fetch-Mode", "navigate");
            web_request.Headers.Add("Sec-Fetch-Dest", "document");
            web_request.Headers.Add("Sec-Fetch-User", "?1");
            web_request.Headers.Add("Upgrade-Insecure-Requests", "1");
            //web_request.Connection = "close";
            web_request.Headers.Add("Accept-Encoding", "gzip, deflate");
            web_request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
                StreamReader streamReader =
                    new StreamReader(httpWebResponse.GetResponseStream());
                string res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();

                string cookie_header = httpWebResponse.GetResponseHeader("Set-Cookie");
                string cf_ray = httpWebResponse.GetResponseHeader("Cf-Ray");
                string[] cf_real_ray = cf_ray.Split(new string[] { "-" }, StringSplitOptions.None);

                Regex dcf_cookie = new Regex("__dcfduid=.{32}");
                Regex sdcf_cookie = new Regex("__sdcfduid=.{96}");

                string dcf = "d";
                string sdcf = "d";

                MatchCollection result_dcf = dcf_cookie.Matches(cookie_header);
                MatchCollection result_sdcf = sdcf_cookie.Matches(cookie_header);

                foreach (Match m in result_dcf)
                {
                    dcf = m.Value;
                }

                foreach (Match m in result_sdcf)
                {
                    sdcf = m.Value;
                }

                Console.WriteLine("Init CF Status code: {0}", httpWebResponse.StatusCode);
                Console.WriteLine("Cf-RAY: " + cf_real_ray[0]);
                Console.WriteLine("CF dcf_cookie: " + dcf);
                Console.WriteLine("CF sdcf_cookie: " + sdcf);
                string[] tes = res.Split(new string[] { ",m:'" }, StringSplitOptions.None);
                string test = tes[1];
                string[] tes2 = test.Split(new string[] { "',s:" }, StringSplitOptions.None);
                Console.WriteLine("test: " + tes2[0]);

                //InitCFBMCookie(cf_real_ray[0], dcf ,sdcf, tes2[0]);

                object[] cf_info = new object[4];
                cf_info[0] = cf_real_ray[0];
                cf_info[1] = dcf;
                cf_info[2] = sdcf;
                cf_info[3] = tes2[0];
                return cf_info;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Phone Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }

            return null;
        }

        private static string InitCFBMCookie(string ray_id, string dcfduid, string sdcfduid, string json_m)
        {
            JObject info = new JObject();
            info.Add("m", json_m);

            JArray results = new JArray();
            results.Add("c66dc54711de60f39fa80adb7b8e57ff");
            results.Add("bb39f9d2a4745eb58bb7c9fedb388f21");
            info.Add("results", results);

            info.Add("timing", 115);

            JObject fp = new JObject();
            fp.Add("id", 3);

            JObject e = new JObject();
            JArray r = new JArray();
            JArray ar = new JArray();
            r.Add(2560);
            r.Add(1440);
            ar.Add(1392);
            ar.Add(2560);
            e.Add("r", r);
            e.Add("ar", ar);
            e.Add("pr", 1);
            e.Add("cd", 24);
            e.Add("wb", true);
            e.Add("wp", false);
            e.Add("wn", false);
            e.Add("ch", true);
            e.Add("ws", false);
            e.Add("wd", false);
            fp.Add("e", e);

            info.Add("fp", fp);

            string post_data = info.ToString(Formatting.None);
            byte[] send_data = UTF8Encoding.UTF8.GetBytes(post_data);

            HttpWebRequest web_request =
                (HttpWebRequest)WebRequest.Create("https://discord.com//cdn-cgi/bm/cv/result?req_id=" + ray_id);
            web_request.Method = "POST";
            web_request.Host = "discord.com";
            web_request.Headers.Add("Cookie",
                dcfduid + "; " + sdcfduid);
            web_request.ContentLength = send_data.Length;
            web_request.Headers.Add("Sec-Ch-Ua", "\"(Not(A:Brand\";v=\"8\", \"Chromium\";v=\"98\"");
            web_request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            web_request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36";
            web_request.ContentType = "application/json";
            web_request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            web_request.Accept = "*/*";
            web_request.Headers.Add("Origin", "https://discord.com");
            web_request.Headers.Add("Sec-Fetch-Site", "same-origin");
            web_request.Headers.Add("Sec-Fetch-Mode", "cors");
            web_request.Headers.Add("Sec-Fetch-Dest", "empty");
            web_request.Referer = "https://discord.com/login";
            web_request.Headers.Add("Accept-Encoding", "gzip, deflate");
            web_request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
            Stream requestStream = web_request.GetRequestStream();
            requestStream.Write(send_data, 0, send_data.Length);
            requestStream.Close();
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
                StreamReader streamReader =
                    new StreamReader(httpWebResponse.GetResponseStream());
                string res = streamReader.ReadToEnd();
                Console.WriteLine("CF BM Status code: {0}", httpWebResponse.StatusCode);

                string cf_bm = httpWebResponse.GetResponseHeader("Set-Cookie");
                string[] cf_bm_split = cf_bm.Split(new string[] { ";" }, StringSplitOptions.None);

                Console.WriteLine("__cf_bm: " + cf_bm_split[0]);
                streamReader.Close();
                httpWebResponse.Close();
                return cf_bm_split[0];
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Phone Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }

            return "";
        }

        private static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        private static String FiveSimGetPhone(string id)
        {
            HttpWebRequest web_request = (HttpWebRequest)WebRequest.Create("https://5sim.net/v1/user/check/" + id);

            web_request.Accept = "application/json";
            web_request.Method = "GET";
            web_request.Headers.Add("Authorization", "Bearer " + fivesim_key);

            HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
            StreamReader streamReader =
                new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            string res = streamReader.ReadToEnd();
            streamReader.Close();
            httpWebResponse.Close();

            JObject get_order = JObject.Parse(res);
            Console.WriteLine(get_order.ToString(Formatting.Indented));

            JArray sms_array = JArray.Parse(get_order["sms"].ToString(Formatting.None));
            if (sms_array.Count == 0)
            {
                return null;
            }
            else
            {
                foreach (JObject sms in sms_array)
                {
                    string code = sms["code"].ToString(Formatting.None);
                    code = code.Replace("\"", "");
                    return code;
                }
            }

            return null;
        }

        private static Object[] FiveSimCreatePhone()
        {
            while (true)
            {
                HttpWebRequest web_request =
                    (HttpWebRequest)WebRequest.Create("https://5sim.net/v1/user/buy/activation/russia/any/discord");

                web_request.Accept = "application/json";
                web_request.Method = "GET";
                web_request.Headers.Add("Authorization", "Bearer " + fivesim_key);

                HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
                StreamReader streamReader =
                    new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                string res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();

                Console.WriteLine(res);
                if (res.Contains("no free"))
                {
                    Console.WriteLine("Failed!");
                    Thread.Sleep(15000);
                }
                else
                {
                    JObject phone = JObject.Parse(res);

                    Object[] phone_info = new object[2];
                    phone_info[0] = phone["id"].ToString(Formatting.None);
                    phone_info[1] = phone["phone"].ToString(Formatting.None);
                    phone_info[1] = phone_info[1].ToString().Replace("\"", "");

                    Console.WriteLine("========Phone Info========");
                    Console.WriteLine("id: " + phone_info[0]);
                    Console.WriteLine("phone: " + phone_info[1]);

                    return phone_info;
                }
            }
        }

        private static void PhoneVericaion2(string token, string password, string phone_token)
        {
            string fingerprint = "945157700084437012.YJaK__wZHPo3LPbUk-h5t5D89Rg";
            JObject info = new JObject();


            string post_data = info.ToString(Formatting.None);

            HttpWebRequest web_request =
                (HttpWebRequest)WebRequest.Create("https://discord.com/api/v9/users/@me/phone");
            byte[] send_data = UTF8Encoding.UTF8.GetBytes(post_data);
            web_request.Method = "POST";
            web_request.Host = "discord.com";
            web_request.Headers.Add("Cookie",
                "__dcfduid=fc9ef620925e11ec94a959325ea420b4; __sdcfduid=fc9ef621925e11ec94a959325ea420b42b0ccff5ee17d5ec38a354098d48588b67dce8793fda9cda83431162758e825a; __cf_bm=AUMkd6_MXNmgrw0KFuRJD3IA1H2mQy.RY.dfaQ__.kY-1645371290-0-AcMi06a9div8Vj2OeBQ/AwcB3OFdSVMWMucEGJlARWm59iTOmoKKy/W1d6Z6uxPlTzSsXZ6ETKVRrteBObOhmURzgEl2PWSgq7T/oiaHuQ5M5NobZwE/0t9x3vpMCByU5NvX2zbCVi8MDHtbuS1KEVk=");
            web_request.ContentLength = send_data.Length;
            web_request.Headers.Add("Sec-Ch-Ua", "\"(Not(A:Brand\";v=\"8\", \"Chromium\";v=\"98\"");
            web_request.Headers.Add("X-Super-Properties",
                "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiQ2hyb21lIiwiZGV2aWNlIjoiIiwic3lzdGVtX2xvY2FsZSI6ImtvLUtSIiwiYnJvd3Nlcl91c2VyX2FnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzk4LjAuNDc1OC44MiBTYWZhcmkvNTM3LjM2IiwiYnJvd3Nlcl92ZXJzaW9uIjoiOTguMC40NzU4LjgyIiwib3NfdmVyc2lvbiI6IjEwIiwicmVmZXJyZXIiOiIiLCJyZWZlcnJpbmdfZG9tYWluIjoiIiwicmVmZXJyZXJfY3VycmVudCI6IiIsInJlZmVycmluZ19kb21haW5fY3VycmVudCI6IiIsInJlbGVhc2VfY2hhbm5lbCI6InN0YWJsZSIsImNsaWVudF9idWlsZF9udW1iZXIiOjExNTYzMywiY2xpZW50X2V2ZW50X3NvdXJjZSI6bnVsbH0=");
            web_request.Headers.Add("X-Fingerprint", fingerprint);
            web_request.Headers.Add("X-Debug-Options", "bugReporterEnabled");
            web_request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            web_request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36";
            web_request.ContentType = "application/json";
            web_request.Headers.Add("X-Discord-Locale", "ko");
            web_request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            web_request.Accept = "*/*";
            web_request.Headers.Add("Origin", "https://discord.com");
            web_request.Headers.Add("Sec-Fetch-Site", "same-origin");
            web_request.Headers.Add("Sec-Fetch-Mode", "cors");
            web_request.Headers.Add("Sec-Fetch-Dest", "empty");
            web_request.Referer = "https://discord.com/channels/@me";
            web_request.Headers.Add("Accept-Encoding", "gzip, deflate");
            web_request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
            web_request.Headers.Add("Authorization", token);

            Stream requestStream = web_request.GetRequestStream();
            requestStream.Write(send_data, 0, send_data.Length);
            requestStream.Close();
            string res = "";
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
                StreamReader streamReader =
                    new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                Console.WriteLine("Phone Status code: {0}", httpWebResponse.StatusCode);
                Console.WriteLine(res);
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Phone Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }
        }

        private static void PhoneVerfication(string token, Object[] phone)
        {
            string code = "";
            while (true)
            {
                code = FiveSimGetPhone(phone[0].ToString());
                if (code != null)
                {
                    Console.WriteLine("Verify Code: " + code);
                    break;
                }
                else
                {
                    Thread.Sleep(10000);
                }
            }

            string fingerprint = "945157700084437012.YJaK__wZHPo3LPbUk-h5t5D89Rg";
            JObject info = new JObject();
            info.Add("phone", phone[1].ToString());
            info.Add("code", code);

            string post_data = info.ToString(Formatting.None);

            HttpWebRequest web_request =
                (HttpWebRequest)WebRequest.Create("https://discord.com/api/v9/phone-verifications/verify");
            byte[] send_data = UTF8Encoding.UTF8.GetBytes(post_data);
            web_request.Method = "POST";
            web_request.Host = "discord.com";
            web_request.Headers.Add("Cookie",
                "__dcfduid=ced3ea10940311ecb380ed0f65b265e6; __sdcfduid=ced3ea11940311ecb380ed0f65b265e635229364c6eb38b4a2e6807dc8642d2363364648bd010e6d9fcb0ee320bcae95; __cf_bm=4j5T85N2IRmJ9zkTMNFxrgOU63YD.IBH_5Wfe8llm2E-1645550462-0-AepHZtPbQ32C0gLOfsYgn4eJJ+2KxPWsXgZy1AzFyfeDJORiDiaoUZH5LUYzdXKORazzNPCDKgBQ+J0rbfZXjuRk0U3jvveRDSkI/7kGjWM8loC5+BQ+cSJYffmbLB/cPA==");
            web_request.ContentLength = send_data.Length;
            web_request.Headers.Add("Sec-Ch-Ua", "\"(Not(A:Brand\";v=\"8\", \"Chromium\";v=\"98\"");
            web_request.Headers.Add("X-Super-Properties",
                "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiQ2hyb21lIiwiZGV2aWNlIjoiIiwic3lzdGVtX2xvY2FsZSI6ImtvLUtSIiwiYnJvd3Nlcl91c2VyX2FnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzk4LjAuNDc1OC4xMDIgU2FmYXJpLzUzNy4zNiIsImJyb3dzZXJfdmVyc2lvbiI6Ijk4LjAuNDc1OC4xMDIiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6MTE1NjMzLCJjbGllbnRfZXZlbnRfc291cmNlIjpudWxsfQ==");
            web_request.Headers.Add("X-Fingerprint", fingerprint);
            web_request.Headers.Add("X-Debug-Options", "bugReporterEnabled");
            web_request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            web_request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            web_request.ContentType = "application/json";
            web_request.Headers.Add("X-Discord-Locale", "ko");
            web_request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            web_request.Accept = "*/*";
            web_request.Headers.Add("Origin", "https://discord.com");
            web_request.Headers.Add("Sec-Fetch-Site", "same-origin");
            web_request.Headers.Add("Sec-Fetch-Mode", "cors");
            web_request.Headers.Add("Sec-Fetch-Dest", "empty");
            web_request.Referer = "https://discord.com/channels/@me";
            web_request.Headers.Add("Accept-Encoding", "gzip, deflate");
            web_request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
            web_request.Headers.Add("Authorization", token);

            Stream requestStream = web_request.GetRequestStream();
            requestStream.Write(send_data, 0, send_data.Length);
            requestStream.Close();
            string res = "";
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
                StreamReader streamReader =
                    new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                Console.WriteLine("Phone Status code: {0}", httpWebResponse.StatusCode);
                Console.WriteLine("Phone Token: " + res);
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }
        }

        private static void PhoneVerificationRequest02(Object[] phone_info, string token, string dcfduid,
            string sdcfduid, string cf_bm)
        {
            Thread.Sleep(7000);
            JObject info = new JObject();
            info.Add("phone", phone_info[1].ToString());

            info.Add("change_phone_reason", "user_action_required");

            string post_data = info.ToString(Formatting.None);

            HttpWebRequest web_request =
                (HttpWebRequest)WebRequest.Create("https://discord.com/api/v9/users/@me/phone");
            byte[] send_data = UTF8Encoding.UTF8.GetBytes(post_data);
            web_request.Method = "POST";
            web_request.Host = "discord.com";
            web_request.Headers.Add("Cookie", dcfduid + "; " + sdcfduid + "; " + cf_bm);
            web_request.ContentLength = send_data.Length;
            web_request.Headers.Add("Sec-Ch-Ua", "\"(Not(A:Brand\";v=\"8\", \"Chromium\";v=\"98\"");
            web_request.Headers.Add("X-Super-Properties",
                "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiQ2hyb21lIiwiZGV2aWNlIjoiIiwic3lzdGVtX2xvY2FsZSI6ImtvLUtSIiwiYnJvd3Nlcl91c2VyX2FnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzk4LjAuNDc1OC4xMDIgU2FmYXJpLzUzNy4zNiIsImJyb3dzZXJfdmVyc2lvbiI6Ijk4LjAuNDc1OC4xMDIiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6MTE1NjMzLCJjbGllbnRfZXZlbnRfc291cmNlIjpudWxsfQ==");
            web_request.Headers.Add("X-Debug-Options", "bugReporterEnabled");
            web_request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            web_request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            web_request.ContentType = "application/json";
            web_request.Headers.Add("X-Discord-Locale", "ko");
            web_request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            web_request.Accept = "*/*";
            web_request.Headers.Add("Origin", "https://discord.com");
            web_request.Headers.Add("Sec-Fetch-Site", "same-origin");
            web_request.Headers.Add("Sec-Fetch-Mode", "cors");
            web_request.Headers.Add("Sec-Fetch-Dest", "empty");
            web_request.Referer = "https://discord.com/channels/@me";
            web_request.Headers.Add("Accept-Encoding", "gzip, deflate");
            web_request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
            web_request.Headers.Add("Authorization", token);

            Stream requestStream = web_request.GetRequestStream();
            requestStream.Write(send_data, 0, send_data.Length);
            requestStream.Close();
            string res = "";
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
                StreamReader streamReader =
                    new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                Console.WriteLine("Phone Status code: {0}", httpWebResponse.StatusCode);
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Phone Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }
        }

        private static void PhoneVerificationRequest01(Object[] phone_info, string token, string dcfduid,
            string sdcfduid, string cf_bm)
        {
            Thread.Sleep(7000);
            JObject info = new JObject();
            info.Add("phone", phone_info[1].ToString());

            string captcha_key = SolveCaptcha(phone_sitekey);
            info.Add("captcha_key", captcha_key);
            info.Add("change_phone_reason", "user_action_required");

            string post_data = info.ToString(Formatting.None);

            HttpWebRequest web_request =
                (HttpWebRequest)WebRequest.Create("https://discord.com/api/v9/users/@me/phone");
            byte[] send_data = UTF8Encoding.UTF8.GetBytes(post_data);
            web_request.Method = "POST";
            web_request.Host = "discord.com";
            web_request.Headers.Add("Cookie", dcfduid + "; " + sdcfduid + "; " + cf_bm);
            web_request.ContentLength = send_data.Length;
            web_request.Headers.Add("Sec-Ch-Ua", "\"(Not(A:Brand\";v=\"8\", \"Chromium\";v=\"98\"");
            web_request.Headers.Add("X-Super-Properties",
                "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiQ2hyb21lIiwiZGV2aWNlIjoiIiwic3lzdGVtX2xvY2FsZSI6ImtvLUtSIiwiYnJvd3Nlcl91c2VyX2FnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzk4LjAuNDc1OC4xMDIgU2FmYXJpLzUzNy4zNiIsImJyb3dzZXJfdmVyc2lvbiI6Ijk4LjAuNDc1OC4xMDIiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6MTE1NjMzLCJjbGllbnRfZXZlbnRfc291cmNlIjpudWxsfQ==");
            web_request.Headers.Add("X-Debug-Options", "bugReporterEnabled");
            web_request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            web_request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            web_request.ContentType = "application/json";
            web_request.Headers.Add("X-Discord-Locale", "ko");
            web_request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            web_request.Accept = "*/*";
            web_request.Headers.Add("Origin", "https://discord.com");
            web_request.Headers.Add("Sec-Fetch-Site", "same-origin");
            web_request.Headers.Add("Sec-Fetch-Mode", "cors");
            web_request.Headers.Add("Sec-Fetch-Dest", "empty");
            web_request.Referer = "https://discord.com/channels/@me";
            web_request.Headers.Add("Accept-Encoding", "gzip, deflate");
            web_request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
            web_request.Headers.Add("Authorization", token);

            Stream requestStream = web_request.GetRequestStream();
            requestStream.Write(send_data, 0, send_data.Length);
            requestStream.Close();
            string res = "";
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
                StreamReader streamReader =
                    new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                Console.WriteLine("Phone Status code: {0}", httpWebResponse.StatusCode);
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Phone Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }
        }

        private static String SendSpoofingPost(string email, string username, string password, string dob,
            string captcha_key, string dcfduid,
            string sdcfduid, string cf_bm)
        {
            string fingerprint = "945157700084437012.YJaK__wZHPo3LPbUk-h5t5D89Rg";

            JObject info = new JObject();
            info.Add("fingerprint", fingerprint);
            info.Add("email", email);
            info.Add("username", username);
            info.Add("password", password);
            info.Add("invite", null);
            info.Add("consent", true);
            info.Add("date_of_birth", dob);
            info.Add("gift_code_sku_id", null);
            info.Add("captcha_key", captcha_key);
            Console.Write(info.ToString(Formatting.Indented));

            string post_data = info.ToString(Formatting.None);

            HttpWebRequest web_request = (HttpWebRequest)WebRequest.Create("https://discord.com/api/v9/auth/register");
            byte[] send_data = UTF8Encoding.UTF8.GetBytes(post_data);
            web_request.Method = "POST";
            web_request.Host = "discord.com";
            web_request.Headers.Add("Cookie", dcfduid + "; " + sdcfduid + "; " + cf_bm);
            web_request.ContentLength = send_data.Length;
            Console.WriteLine(send_data.Length);
            web_request.Headers.Add("Sec-Ch-Ua", "\"(Not(A:Brand\";v=\"8\", \"Chromium\";v=\"98\"");
            web_request.Headers.Add("X-Super-Properties",
                "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiQ2hyb21lIiwiZGV2aWNlIjoiIiwic3lzdGVtX2xvY2FsZSI6ImtvLUtSIiwiYnJvd3Nlcl91c2VyX2FnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzk4LjAuNDc1OC44MiBTYWZhcmkvNTM3LjM2IiwiYnJvd3Nlcl92ZXJzaW9uIjoiOTguMC40NzU4LjgyIiwib3NfdmVyc2lvbiI6IjEwIiwicmVmZXJyZXIiOiIiLCJyZWZlcnJpbmdfZG9tYWluIjoiIiwicmVmZXJyZXJfY3VycmVudCI6IiIsInJlZmVycmluZ19kb21haW5fY3VycmVudCI6IiIsInJlbGVhc2VfY2hhbm5lbCI6InN0YWJsZSIsImNsaWVudF9idWlsZF9udW1iZXIiOjExNTYzMywiY2xpZW50X2V2ZW50X3NvdXJjZSI6bnVsbH0=");
            web_request.Headers.Add("X-Fingerprint", fingerprint);
            web_request.Headers.Add("X-Debug-Options", "bugReporterEnabled");
            web_request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            web_request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36";
            web_request.ContentType = "application/json";
            web_request.Headers.Add("X-Discord-Locale", "ko");
            web_request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            web_request.Accept = "*/*";
            web_request.Headers.Add("Origin", "https://discord.com");
            web_request.Headers.Add("Sec-Fetch-Site", "same-origin");
            web_request.Headers.Add("Sec-Fetch-Mode", "cors");
            web_request.Headers.Add("Sec-Fetch-Dest", "empty");
            web_request.Referer = "https://discord.com/register";
            web_request.Headers.Add("Accept-Encoding", "gzip, deflate");
            web_request.Headers.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");

            Stream requestStream = web_request.GetRequestStream();
            requestStream.Write(send_data, 0, send_data.Length);
            requestStream.Close();
            string res = "";
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
                StreamReader streamReader =
                    new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();

                Console.WriteLine("Register Status code: {0}", httpWebResponse.StatusCode);

                JObject result = JObject.Parse(res);
                string status = result["token"].ToString();
                Console.WriteLine(status);
                //FakeScienceRequest000(status);
                return status;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Register Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }

            return "";
        }

        private static String AntiGetTask(string client_key, int task_id)
        {
            while (true)
            {
                JObject get_task = new JObject();
                get_task.Add("clientKey", client_key);
                get_task.Add("taskId", task_id);

                string post_data = get_task.ToString(Formatting.Indented);

                HttpWebRequest web_request =
                    (HttpWebRequest)WebRequest.Create("https://api.anti-captcha.com/getTaskResult");
                byte[] send_data = UTF8Encoding.UTF8.GetBytes(post_data);
                web_request.ContentType = "application-json";
                web_request.Method = "POST";
                web_request.ContentLength = send_data.Length;
                Stream requestStream = web_request.GetRequestStream();
                requestStream.Write(send_data, 0, send_data.Length);
                requestStream.Close();
                HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
                StreamReader streamReader =
                    new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                string res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();

                JObject result = JObject.Parse(res);
                string status = result["status"].ToString();
                Console.WriteLine(status);
                if (status == "ready")
                {
                    return res;
                }
                else
                {
                    Thread.Sleep(10000);
                }
            }
        }

        private static int AntiCreateTask(string client_key, string type, string url, string sitekey)
        {
            JObject create_task = new JObject();
            create_task.Add("clientKey", client_key);

            JObject task_json = new JObject();
            task_json.Add("type", type);
            task_json.Add("websiteURL", url);
            task_json.Add("websiteKey", sitekey);
            create_task.Add("task", task_json);

            create_task.Add("softId", 0);
            create_task.Add("languagePool", "en");

            string post_data = create_task.ToString(Formatting.Indented);

            HttpWebRequest web_request = (HttpWebRequest)WebRequest.Create("https://api.anti-captcha.com/createTask");
            byte[] send_data = UTF8Encoding.UTF8.GetBytes(post_data);
            web_request.ContentType = "application-json";
            web_request.Method = "POST";
            web_request.ContentLength = send_data.Length;
            Stream requestStream = web_request.GetRequestStream();
            requestStream.Write(send_data, 0, send_data.Length);
            requestStream.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)web_request.GetResponse();
            StreamReader streamReader =
                new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            string res = streamReader.ReadToEnd();
            streamReader.Close();
            httpWebResponse.Close();

            JObject result = JObject.Parse(res);
            int error_id = Int32.Parse(result["errorId"].ToString());
            if (error_id == 0)
            {
                int task_id = Int32.Parse(result["taskId"].ToString());
                return task_id;
            }
            else
            {
                Console.WriteLine(res);
                Console.WriteLine("ERROR!");
                return -1;
            }
        }

        private static String SolveCaptcha(string sitekey)
        {
            string type = "HCaptchaTaskProxyless";
            string url = "https://discord.com/";

            int result = AntiCreateTask(anti_key, type, url, sitekey);
            if (result != -1)
            {
                Thread.Sleep(10000);

                string json = AntiGetTask(anti_key, result);

                JObject parent_json = JObject.Parse(json);
                string solution = parent_json["solution"].ToString(Formatting.None);

                JObject child_json = JObject.Parse(solution);
                string captcha_key = child_json["gRecaptchaResponse"].ToString(Formatting.None);
                captcha_key = captcha_key.Replace("\"", "");
                return captcha_key;
            }

            return null;
        }

        private static string RandomString(int _nLength)
        {
            const string strPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            char[] chRandom = new char[_nLength];

            for (int i = 0; i < _nLength; i++)
            {
                chRandom[i] = strPool[random.Next(strPool.Length)];
            }

            string strRet = new String(chRandom);
            return strRet;
        }
    }
}