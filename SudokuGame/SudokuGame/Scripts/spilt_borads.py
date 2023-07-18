import os
import json

# Get the file paths
path_folder = "bin/debug/borads_game/"
current_directory = os.path.dirname(os.path.abspath(__file__))
parent_directory = os.path.dirname(current_directory)
txt_file_path = os.path.join(parent_directory, path_folder, "boards.txt")
json_file_path = os.path.join(parent_directory, path_folder, "boards.json")

# Read the text file content
with open(txt_file_path, "r") as file:
    content = file.read()

# Parse the content and convert it into a dictionary
difficulty_boards = {}
difficulty = ""
board = []
for line in content.split("\n"):
    line = line.strip()
    if line.startswith("="):
        difficulty_boards[difficulty].append(board)
        board = []
    elif line == '':
        difficulty_boards[difficulty].append(board)
        board = []
    else:
        if line.startswith("{") and line.endswith("}"):
            board.append(list(map(int, line[1:-1].split())))
        else:
            difficulty = line
            difficulty_boards[difficulty] = []

# Save the dictionary to a JSON file
with open(json_file_path, "w") as json_file:
    for difficulty, board in difficulty_boards.items():
        json.dump({difficulty: board}, json_file)
        json_file.write('\n')


# Serialize the dictionary into a pickle file
# with open(pickle_file_path, "wb") as file:
#     pickle.dump(difficulty_boards, file)
