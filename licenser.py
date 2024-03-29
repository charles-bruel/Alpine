#>============================================================================<
#
#    Alpine, Ski Resort Tycoon Game
#    Copyright (C) 2024  Charles Bruel
#
#    This program is free software: you can redistribute it and/or modify
#    it under the terms of the GNU General Public License as published by
#    the Free Software Foundation, either version 3 of the License, or
#    (at your option) any later version.
#
#    This program is distributed in the hope that it will be useful,
#    but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#    GNU General Public License for more details.
#
#    You should have received a copy of the GNU General Public License
#    along with this program.  If not, see <https://www.gnu.org/licenses/>.
#>============================================================================<

import glob

text = """>============================================================================<

    Alpine, Ski Resort Tycoon Game
    Copyright (C) 2024  Charles Bruel

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
>============================================================================<"""

file_types = [("*.cs", "//"), ("*.py", "#"), ("*.shader", "//")]
search_dirs = ["Assets/Scripts/**", "Assets/ModAPI/**", "Assets/Game Elements/**"]

count = 0

for file_type in file_types:
    modified_text = "\n".join([file_type[1] + line for line in text.split("\n")])
    print(modified_text)

    for search_dir in search_dirs:
        for file_path in glob.glob(search_dir + "/" + file_type[0], recursive=True):
            with open(file_path, "r") as f:
                contents = f.read()
                if modified_text in contents:
                    continue
            with open(file_path, "w") as f:
                f.write(modified_text + "\n\n" + contents)
                count += 1

print(f"Added license to {count} files.")
