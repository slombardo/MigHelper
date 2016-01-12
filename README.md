# MigHelper
This console application is a crude but effective tool to help recover files from corrupted Microsoft MIG backup file.  

After trying numerous "solutions" I was still not able to recover my lost files.  One such solution was [MigRecover](https://www.visser.com.au/blog/migrecover-for-microsofts-windows-easy-transfer/)
which resulted in the following error:

```
Error while trying to open the source MIG file. Error: 0x000003EE
```

Finally I gave up and wrote this little application and recovered all 85 gb of files.

## Instructions
1. Download and extract this zip file: [MigHelper.zip](http://slombardo.github.io/MigHelper/MigHelper.zip)
  - Alternatively, clone and compile this repository (and contribute/comment for any issues or questions).
- Open a Command Prompt window
  1. click the Start Menu
  2. type: `cmd`and press the `Enter` key
- To navigate to the MigHelper directory you just unzipped:
  - type: `cd "C:\MigHelper"` and press `Enter` key
- Type: `MigHelper.exe` and press the `Enter` key
- Enter the full path of the the MIG file to recover and press the `Enter` key
- Select a valid restore point and press the `Enter` key
  - The program will only restore files up to the restore point you selected.  Inspect the restore points' names as they indicate the dates you will be restoring up to.
  - Choosing the oldest version will restore everything up to that point.
- That's it!
