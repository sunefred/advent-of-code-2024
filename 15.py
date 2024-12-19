from collections import deque
import os

DIRECTIONS = {
    "<": (0, -1),
    ">": (0, 1),
    "^": (-1, 0),
    "v": (1, 0),
}
DIR_PATH = os.path.dirname(os.path.realpath(__file__))

def read_input(test=False) -> str:
    filename = f"{DIR_PATH}/dotnet/Day15/Data/input.txt"
    with open(filename) as f:
        input_ = f.read()
    return input_

def solve():
    wmap, move = read_input(1).split("\n\n")
    grid = []

    pos = None
    for r, line in enumerate(wmap.split("\n")):
        row = []
        for c, p in enumerate(line):
            row.append(p)
            if p == "@":
                pos = (r, c)
        grid.append(row)

    moves = [m for line in move.split("\n") for m in line]

    for move in moves:
        cur_train = []
        dr, dc = DIRECTIONS[move]

        tr, tc = pos
        ok = False
        parts = 0
        while True:
            if grid[tr][tc] in {"@", "O"}:
                cur_train.append((tr, tc))
                parts += 1
            elif grid[tr][tc] == "#":
                break
            else:
                assert grid[tr][tc] == "."
                ok = True
                break
            tr += dr
            tc += dc
        if not ok:
            continue
        for pr, pc in cur_train[::-1]:
            if grid[pr][pc] == "@":
                pos = (pos[0] + dr, pos[1] + dc)
            grid[pr + dr][pc + dc] = grid[pr][pc]
            grid[pr][pc] = "."
    print(grid)
    ans = 0
    for r, row in enumerate(grid):
        for c, p in enumerate(row):
            if p == "O":
                ans += 100 * r + c
    print("Ans", ans)


def solve_p2():
    wmap, move = read_input().split("\n\n")
    grid = []
    pos = None
    for r, line in enumerate(wmap.split("\n")):
        row = []
        for c, p in enumerate(line):
            if p == "#":
                row.extend(["#", "#"])
            elif p == "O":
                row.extend(["[", "]"])
            elif p == ".":
                row.extend([".", "."])
            elif p == "@":
                row.extend(["@", "."])
                pos = (r, c * 2)
        grid.append(row)

    moves = [m for line in move.split("\n") for m in line]

    for move in moves:
        dr, dc = DIRECTIONS[move]
        ok, cur_train = get_train(grid, move, pos)
        if not ok:
            continue
        for pr, pc in cur_train[::-1]:
            if grid[pr][pc] == "@":
                pos = (pos[0] + dr, pos[1] + dc)
            grid[pr + dr][pc + dc] = grid[pr][pc]
            grid[pr][pc] = "."
    for row in grid:
        print(row)

    ans = 0
    for r, row in enumerate(grid):
        for c, p in enumerate(row):
            if p in "[":
                ans += 100 * r + c
    print("Ans", ans)


def get_train(grid, move, pos):
    """
    get all the boxes that should move, dfs to remember the order
    ##############          ##############
    ##......##..##          ##......##..##
    ##..........##          ##...[][]...##
    ##...[][]...##   -->    ##....[]....##
    ##....[]....##          ##.....@....##
    ##.....@....##          ##..........##
    ##############          ##############
    """
    q = deque([pos])
    seen = set()
    cur_train = []
    dr, dc = DIRECTIONS[move]
    while q:
        r, c = q.popleft()
        if (r, c) in seen:
            continue
        seen.add((r, c))
        if grid[r][c] == ".":
            continue
        elif grid[r][c] == "#":
            return False, []
        cur_train.append((r, c))
        cur_p = grid[r][c]
        assert cur_p in {"[", "]", "@"}
        if cur_p == "[":
            if (r, c + 1) not in seen and move != ">":
                q.append((r, c + 1))
        elif cur_p == "]":
            if (r, c - 1) not in seen and move != "<":
                q.append((r, c - 1))
        q.append((r + dr, c + dc))
    return True, cur_train


if __name__ == "__main__":
    #solve()
    solve_p2()