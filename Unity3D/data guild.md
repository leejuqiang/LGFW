### Data Configuration
##### Code
This framework support spread sheets as configurations of data. Let's take an example, in your Unity3D, you have 2 classes like this:
```
[System.Serializable]
class MyData {
    public int m_a;
    public int m_b;
    public string[] m_text;
}

class MyDataDB : ScriptableObject {
    public List<MyData> m_dataList;
}
```
MyData is the class of the data, and MyDataDB is the class of the asset. Your goal is to configure the list of MyData (which is the m_dataList in MyDataDB) by a spread sheet.  
One rule you must follow is the name of MyDataDB. It has to be the name of the data class (in this case, "MyData") plus "DB". Because the processor can only know the name of the data class, and the name the List of the data class must be m_dataList.

##### Spread Sheet
In your spread sheet, the name of each sheet is the data class name. The first non-empty row of a sheet is the header, the header is used to determine the name of each column. Once you have the header, you assign a name to each column, this name is use to match the names of fields in the data class. So your sheet may looks like this:

| a      | b      | text       |
| ------ | ------ | -------- |
| 1      | 1      | ttttt    |
| 2      | 2      | aaaaa    |

You don't need to use m_a in the header, just use a, the processor will search m_a, if not found, then search a.  
So now, you configure 2 MyData, with the value {m_a:1， m_b:1， m_text:[ttttt]} and {m_a:2， m_b:2， m_text:[aaaaa]}. Because m_text
is an array, the processor splits the string by the default splitting string " " (one space character). You can also specific the splitting string by using the attribute [DataSplit(" ,.")] (in this case, the spliting string is " ,.").

##### Merged cells
You can use merged cell in your spread sheet. Your sheet may looks like this:

<table>
  <tr>
    <td colspan="2">a</td>
    <td>b</td>
    <td colspan="2">text</td>
  </tr>
  <tr>
    <td>1</td>
    <td>1</td>
    <td>1</td>
    <td>ttttt</td>
    <td>123</td>
  </tr>
</table>

If you use a merged cell in header, that means those columns should be treated as one column. If the field of that column expects one string as the value (such as m_a, which is an int), the value is the combination of all those columns. So in this case, the value of m_a is 11. You can also use [DataCombineText("0")] to add a custom string when joining columns. In this case, if you use "0", m_a is 101. Or you can use [DontCombineText] to specific that use the value of the first cell instead of using the combine of all cells.  
If the field excepts an array. And you don't ues [DataSplit] (if you use, the array is the spliting of the combined string), then those columns are treated as an array. So the array has the same length of the columns, each element in the array is the value of each column.  
Don't use merged cells span different header.

### Setup
Your spread sheet must be under the folder "Assets/Data", and using a extension ".xls" or ".xlsx".  
You must install python3 and the package "xlrd". Make sure you set up the environment variables correct. If you have both python3 and python2，make sure that you installed "xlrd" for the right version and have python linked to python3.  
If you're using a macos, after you installed python3, open your terminal, use cd to the folder "Assets/LGFW/Editor/", make sure you are under this folder, then:  
$ chmod +x init.sh  
$ init.sh  
This creates a file "parseExcel.sh" under the same folder. You can check the file to see if the path of your python3 inside it is correct.

Now you are all set. Each time you update your spread sheet, the processor automatically update files {spread sheet name}\_{sheet name}.json and {spread sheet name}\_{sheet name}.asset. If you have a spread sheet "test.xls", in this file, you have 2 sheets "MyData1" and "MyData2", then 4 files are updated, "test_MyData1.json", "test_MyData2.json", "test_MyData1.asset"  and ""test_MyData2.asset".  
.json file is the json format of the spread sheet. .asset is the data you use in Unity3D.

### Localization
To generate localization data, you need to set the sheet name to "Localization" (not the spread sheet name). The column named "id" is treated as the text id. And each column with another name is treated as a language. Each of them is output as a single asset, with the name of {column name}.asset. See an example:

| id      | en      | en1       |
| ------ | ------ | -------- |
| hello      | hello      | hi    |
| ok      | ok      | okay    |

2 .asset fiels are outputed, "en.asset" and "en1.asset", they are assets with type LocalizedText.
