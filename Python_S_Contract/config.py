from datetime import timedelta
from dotenv import load_dotenv
import os

load_dotenv()

# ACCESS_EXPIRES = timedelta(hours=3)


MANAGER_ADDRESS  = os.getenv("MANAGER_ADDRESS")
CONTRACT_ADDRESS = os.getenv("CONTRACT_ADDRESS")
PRIVATE_KEY = os.getenv("PRIVATE_KEY")
INFURA_API = os.getenv("INFURA_API")
# JWT_ACCESS_TOKEN_EXPIRES = ACCESS_EXPIRES

# MAIL_USERNAME = os.getenv("MAIL_USERNAME")
# PASSWORD = os.getenv("PASSWORD")
# MAIL_CONFIG = {
#     'MAIL_SERVER': 'smtp.gmail.com',  # example server for Gmail
#     'MAIL_PORT': 465,  # example port for Gmail
#     'MAIL_USERNAME': MAIL_USERNAME,
#     'MAIL_PASSWORD': PASSWORD,  # replace this with the actual password
#     'MAIL_USE_TLS': False,  # use TLS encryption
#     'MAIL_USE_SSL': True  # do not use SSL encryption
# }
