import xlrd
import sys
import os
import json
import datetime

dir, fullFile = os.path.split(sys.argv[1])
file, ext = os.path.splitext(fullFile)
wb = None


def escapeChar(ch):
    if ch == "n":
        return "\n"
    if ch == "r":
        return "\r"
    if ch == "t":
        return "\t"
    return ch


def formatNumber(s):
    f = float(s)
    i = int(f)
    if i == f:
        return str(i)
    return s


def readCellValue(cell):
    t = cell.ctype
    s = cell.value
    if t == xlrd.XL_CELL_ERROR:
        s = ""
    elif t == xlrd.XL_CELL_NUMBER:
        s = formatNumber(s)
    elif t == xlrd.XL_CELL_DATE:
        s = str(datetime.datetime(*xlrd.xldate_as_tuple(s, wb.datemode)))
    else:
        s = str(s).rstrip()
    ret = ""
    escape = False
    for i in range(len(s)):
        if escape:
            ret += escapeChar(s[i])
            escape = False
        else:
            if s[i] == "\\":
                escape = True
            else:
                ret += s[i]
    return ret


def isEmptyRow(sheet, row):
    for i in range(sheet.ncols):
        if readCellValue(sheet.cell(row, i)) != "":
            return False
    return True


def readCell(sheet, r, c, mergeMap):
    e = c + 1
    if r in mergeMap:
        dict = mergeMap[r]
        if c in dict:
            e = dict[c]
    ret = ""
    for i in range(c, e):
        ret += readCellValue(sheet.cell(r, i))
    return ret, e


def parseSheet(sheet):
    dict = {}
    sname = sheet.name
    last = sheet.name.rfind(".")
    if last >= 0:
        sname = sname[last + 1:]

    dict["name"] = file + "_" + sname
    dict["class"] = sheet.name

    outPath = os.path.join(dir, file + "_" + sname + ".json")
    data = []

    mergeMap = {}
    for mc in sheet.merged_cells:
        for r in range(mc[0], mc[1]):
            l = {}
            if r in mergeMap:
                l = mergeMap[r]
            else:
                mergeMap[r] = l
            l[mc[2]] = mc[3]

    heads = {}
    headers = []
    r = 0
    for r in range(sheet.nrows):
        if not isEmptyRow(sheet, r):
            break

    if r >= sheet.nrows:
        return

    c = 0
    while c < sheet.ncols:
        s, e = readCell(sheet, r, c, mergeMap)
        if s != "":
            h = (s, c, e)
            heads[c] = h
            headers.append(s)
        c = e
    dict["header"] = headers

    r += 1
    while r < sheet.nrows:
        if not isEmptyRow(sheet, r):
            row = {}
            c = 0
            while c < sheet.ncols:
                s, e = readCell(sheet, r, c, mergeMap)
                if c in heads:
                    h = heads[c]
                    v = [s]
                    while e < h[2]:
                        s, e = readCell(sheet, r, e, mergeMap)
                        v.append(s)
                    row[h[0]] = v
                c = e
            data.append(row)
        r += 1
    dict["data"] = data

    js = json.dumps(dict, ensure_ascii=False)
    with open(outPath, "w", encoding="utf-8") as wf:
        wf.write(js)


isXls = ext == ".xls"
wb = xlrd.open_workbook(sys.argv[1], formatting_info=isXls)

for s in range(wb.nsheets):
    parseSheet(wb.sheet_by_index(s))
