from flask import Flask, request,render_template
from web3 import Web3
from eth_account import Account
from eth_keys import keys

# from contractAbi import contractABI

from dotenv import load_dotenv
load_dotenv('.env')

import json
import hashlib





# print(contract_abi)

app = Flask(__name__)

contract_abi = []
with open('contractAbi.json', 'r') as f:
    contract_abi = json.load(f)

app.config.from_object('config')

print(app.config["MANAGER_ADDRESS"],app.config["INFURA_API"])
web3 = Web3(Web3.HTTPProvider(app.config["INFURA_API"]))  # Replace with your Infura endpoint or Ethereum node URL


manager_address = app.config["MANAGER_ADDRESS"]

gas_price_wei = web3.to_wei(0.00000005, 'ether')


# Contract address and ABI
contract_address = app.config["CONTRACT_ADDRESS"]


# Create contract instance
contract = web3.eth.contract(address=contract_address, abi=contract_abi)

@app.route('/',methods=['GET'])
def home():

    data = request.get_json()
    t = get_Balance(data["account"])
    # balance = contract.functions.balanceOf(data["account"]).call()
    
    return f"<h3>balance :{t} </h3>"

#-----------------------------------------------------------

def get_Balance(account):
    print(account)
    return contract.functions.balanceOf(account).call()

# Encrypt account number
def encrypt_account(account):
    account_bytes = account.encode('utf-8')
    encrypted_account = hashlib.sha256(account_bytes).hexdigest()
    return encrypted_account

# Check if account number already exists in JSON file
def account_exists(account):
    with open('account.json', 'r') as file:
        data = json.load(file)
        return account in data

# Save account number to JSON file
def save_account(account):
    with open('account.json', 'r') as file:
        accounts = json.load(file)
    
    accounts[account] = account  # Assign a value to the account key
    
    with open('account.json', 'w') as file:
        json.dump(accounts, file)  # Save the updated dictionary back to the file

# Mint route
@app.route('/mint', methods=['POST'])
def mint():
    data = request.get_json()
    account = data['account']
    amount = int(data['amount'])  # Convert the amount to an integer

    # Check if the caller is the manager account
    if web3.eth.default_account and web3.eth.default_account.lower() != manager_address.lower():
        return "Only the manager can call this function"
#-----------------------
      # Encrypt the account number
    encrypted_account = encrypt_account(account)

    # Check if account number already exists in JSON file
    if account_exists(encrypted_account):
      
        if int(get_Balance(account)) == 0 :
            return "The User exist but dont have money",403
        return "Account number already exists and have money",201

    # Save the account number to JSON file
    save_account(encrypted_account)
#----------------------------
    # Prepare the transaction
    contract_function = contract.functions.mint(account, amount)

    # Build the transaction parameters
    tx_params = {
        'from': manager_address,
        'nonce': web3.eth.get_transaction_count(manager_address),
        'gas': 300000,  # Adjust the gas limit as needed
        'gasPrice': 50000000000,  # Set the gas price in Gwei (adjust as necessary)
    }

    # Estimate the gas required for the transaction
    gas_estimate = contract_function.estimate_gas(tx_params)

    # Update the gas parameter if the estimate is higher
    if gas_estimate > tx_params['gas']:
        tx_params['gas'] = gas_estimate

    # Build the transaction
    signed_txn = contract_function.build_transaction(tx_params)

    # Sign the transaction
    signed_txn = web3.eth.account.sign_transaction(signed_txn, private_key=app.config["PRIVATE_KEY"])

    # Send the signed transaction
    tx_hash = web3.eth.send_raw_transaction(signed_txn.rawTransaction)

    # Wait for the transaction to be mined
    receipt = web3.eth.wait_for_transaction_receipt(tx_hash)

    # Get the network name
    network = web3.eth.chain_id

    # Construct the etherscan URL
    etherscan_url = f"https://sepolia.etherscan.io/tx/{tx_hash.hex()}"

    return f"Minted {amount} SE tokens to {account}. Transaction URL: <a href='{etherscan_url}'>{etherscan_url}</a>" ,200



@app.route('/transfer', methods=['POST'])
def transfer():
    try:

        data = request.get_json()
        account = data['account']
        amount = int(data['amount'])  # Convert the amount to an integer

        # Check if the caller is the manager account
        if web3.eth.default_account and web3.eth.default_account.lower() != manager_address.lower():
            return "Only the manager can call this function"

        # Prepare the transaction
        contract_function = contract.functions.transfer(account, amount)

        # Build the transaction parameters
        tx_params = {
            'from': manager_address,
            'nonce': web3.eth.get_transaction_count(manager_address),
            'gas': 300000,  # Adjust the gas limit as needed
            'gasPrice': 50000000000,  # Set the gas price in Gwei (adjust as necessary)
        }

        # Estimate the gas required for the transaction
        gas_estimate = contract_function.estimate_gas(tx_params)

        # Update the gas parameter if the estimate is higher
        if gas_estimate > tx_params['gas']:
            tx_params['gas'] = gas_estimate

        # Build the transaction
        signed_txn = contract_function.build_transaction(tx_params)

        # Sign the transaction
        signed_txn = web3.eth.account.sign_transaction(signed_txn, private_key=app.config["PRIVATE_KEY"])

        # Send the signed transaction
        tx_hash = web3.eth.send_raw_transaction(signed_txn.rawTransaction)

        # Wait for the transaction to be mined
        receipt = web3.eth.wait_for_transaction_receipt(tx_hash)

        # Get the network name
        network = web3.eth.chain_id

        # Construct the etherscan URL
        etherscan_url = f"https://sepolia.etherscan.io/tx/{tx_hash.hex()}"

        return f"Transferred {amount} tokens to {account}. Transaction URL: <a href='{etherscan_url}'>{etherscan_url}</a>"
        
    except Exception as e: 
      
        # Handle the specific exception and return the desired status code
        return str(e), 500

@app.route('/transferFrom', methods=['POST'])
def transfer_from():
    data = request.get_json()
    from_account = data['from']
    to_account = data['to']
    amount = int(data['amount'])

    # Validate that the caller is the administrator
    if web3.eth.default_account and web3.eth.default_account.lower() != manager_address.lower():
        return "Only the manager can call this function"

    # Prepare the transaction for transferFrom
    transfer_from_function = contract.functions.transferFrom(from_account, to_account, amount)
    transfer_from_transaction = transfer_from_function.build_transaction({
        'from': manager_address,
        'nonce': web3.eth.get_transaction_count(manager_address),
        'gas': 300000,
        'gasPrice': 50000000000
    })

    # Sign the transaction with the administrator's private key
    signed_txn = web3.eth.account.sign_transaction(transfer_from_transaction, private_key=app.config["PRIVATE_KEY"])

    # Send the signed transaction
    tx_hash = web3.eth.send_raw_transaction(signed_txn.rawTransaction)

    # Wait for the transaction to be mined
    receipt = web3.eth.wait_for_transaction_receipt(tx_hash)

    # Get the network name
    network = web3.eth.chain_id

    # Construct the etherscan URL
    etherscan_url = f"https://sepolia.etherscan.io/tx/{tx_hash.hex()}"

    return f"Transferred {amount} tokens from {from_account} to {to_account}. Transaction URL: <a href='{etherscan_url}'>{etherscan_url}</a>"






if __name__ == '__main__':
    app.run(debug=True,port=5000)
