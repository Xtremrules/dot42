// psvince.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"

#include <tlhelp32.h>
#include <vdmdbg.h>

typedef struct
{
   DWORD          dwPID;
   DWORD          lParam;
   BOOL           bEnd;
} EnumInfoStruct;

BOOL WINAPI EnumProcs( char* procname );


BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
    return TRUE;
}

int APIENTRY IsModuleLoaded( char *lpModule )
{
 return EnumProcs( lpModule );
}



BOOL WINAPI EnumProcs( char* procname )
{

	OSVERSIONINFO  osver ;
	HINSTANCE      hInstLib ;
	// HINSTANCE      hInstLib2 ;
	HANDLE         hSnapShot ;
	PROCESSENTRY32 procentry ;
	BOOL           bFlag ;
	LPDWORD        lpdwPIDs ;
	DWORD          dwSize, dwSize2, dwIndex ;
	HMODULE        hMod ;
	HANDLE         hProcess ;
	char           szFileName[ MAX_PATH ] ;

	char*	nomemodulo;

	bool retcode = false;

//	EnumInfoStruct sInfo ;

    // ToolHelp Function Pointers.

	HANDLE (WINAPI *lpfCreateToolhelp32Snapshot)(DWORD,DWORD) ;
	BOOL (WINAPI *lpfProcess32First)(HANDLE,LPPROCESSENTRY32) ;
	BOOL (WINAPI *lpfProcess32Next)(HANDLE,LPPROCESSENTRY32) ;

    // PSAPI Function Pointers.

	BOOL (WINAPI *lpfEnumProcesses)( DWORD *, DWORD cb, DWORD * );
	BOOL (WINAPI *lpfEnumProcessModules)( HANDLE, HMODULE *, DWORD, LPDWORD );
	DWORD (WINAPI *lpfGetModuleFileNameEx)( HANDLE, HMODULE, LPTSTR, DWORD );


    // VDMDBG Function Pointers.
    // INT (WINAPI *lpfVDMEnumTaskWOWEx)( DWORD, TASKENUMPROCEX  fp, LPARAM );


	// Check to see if were running under Windows95 or
    // Windows NT.

    osver.dwOSVersionInfoSize = sizeof( osver ) ;
    if( !GetVersionEx( &osver ) )
    {
		return FALSE ;
    }

    // rendo minuscolo il nome del modulo
	_strlwr( procname );

    // If Windows NT:
    if( osver.dwPlatformId == VER_PLATFORM_WIN32_NT )
    {
		// Load library and get the procedures explicitly. We do
		// this so that we don't have to worry about modules using
		// this code failing to load under Windows 95, because
		// it can't resolve references to the PSAPI.DLL.

		hInstLib = LoadLibraryA( "PSAPI.DLL" ) ;
		if( hInstLib == NULL ) return FALSE ;

        // hInstLib2 = LoadLibraryA( "VDMDBG.DLL" ) ;
        // if( hInstLib2 == NULL ) return FALSE ;

        // Get procedure addresses.
        lpfEnumProcesses = (BOOL(WINAPI *)(DWORD *,DWORD,DWORD*)) GetProcAddress( hInstLib, "EnumProcesses" ) ;
        lpfEnumProcessModules = (BOOL(WINAPI *)(HANDLE, HMODULE *, DWORD, LPDWORD)) GetProcAddress( hInstLib, "EnumProcessModules" ) ;
		lpfGetModuleFileNameEx =(DWORD (WINAPI *)(HANDLE, HMODULE, LPTSTR, DWORD )) GetProcAddress( hInstLib, "GetModuleFileNameExA" ) ;
		//lpfVDMEnumTaskWOWEx =(INT(WINAPI *)( DWORD, TASKENUMPROCEX, LPARAM)) GetProcAddress( hInstLib2, "VDMEnumTaskWOWEx" );

		if( lpfEnumProcesses == NULL || lpfEnumProcessModules == NULL ||
			lpfGetModuleFileNameEx == NULL ) // || lpfVDMEnumTaskWOWEx == NULL)
        {
			FreeLibrary( hInstLib ) ;
			//FreeLibrary( hInstLib2 ) ;
            return FALSE ;
        }


		// Call the PSAPI function EnumProcesses to get all of the
        // ProcID's currently in the system.
        // NOTE: In the documentation, the third parameter of
        // EnumProcesses is named cbNeeded, which implies that you
        // can call the function once to find out how much space to
        // allocate for a buffer and again to fill the buffer.
        // This is not the case. The cbNeeded parameter returns
        // the number of PIDs returned, so if your buffer size is
        // zero cbNeeded returns zero.
        // NOTE: The "HeapAlloc" loop here ensures that we
        // actually allocate a buffer large enough for all the
        // PIDs in the system.

		dwSize2 = 256 * sizeof( DWORD ) ;
        lpdwPIDs = NULL ;

        do
		{

			if( lpdwPIDs )
            {
                HeapFree( GetProcessHeap(), 0, lpdwPIDs ) ;
                dwSize2 *= 2 ;
             }

             lpdwPIDs = (LPDWORD)HeapAlloc( GetProcessHeap(), 0, dwSize2 );
             if( lpdwPIDs == NULL )
             {
                FreeLibrary( hInstLib ) ;
                //FreeLibrary( hInstLib2 ) ;
                return FALSE ;
             }
             
			 if( !lpfEnumProcesses( lpdwPIDs, dwSize2, &dwSize ) )
             {
                HeapFree( GetProcessHeap(), 0, lpdwPIDs ) ;
                FreeLibrary( hInstLib ) ;
                //FreeLibrary( hInstLib2 ) ;
                return FALSE ;
             }

        } while( dwSize == dwSize2 ) ;

        // How many ProcID's did we get?
        dwSize /= sizeof( DWORD ) ;

        // Loop through each ProcID.
        for( dwIndex = 0 ; dwIndex < dwSize ; dwIndex++ )
        {
			szFileName[0] = 0 ;

            // Open the process (if we can... security does not
            // permit every process in the system).
            hProcess = OpenProcess(
            PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, lpdwPIDs[ dwIndex ] ) ;

            if( hProcess != NULL )
            {
				// Here we call EnumProcessModules to get only the
                // first module in the process this is important,
                // because this will be the .EXE module for which we
                // will retrieve the full path name in a second.
                if( lpfEnumProcessModules( hProcess, &hMod, sizeof( hMod ), &dwSize2 ) )
                {

                   // Get Full pathname:
                   if( !lpfGetModuleFileNameEx( hProcess, hMod, szFileName, sizeof( szFileName ) ) )
                   {
                      szFileName[0] = 0 ;
                   }

					// rendo minuscolo il modulo
					_strlwr( szFileName );
					// estraggo il filename
					nomemodulo = strrchr( szFileName, '\\' );
					if ( nomemodulo ) nomemodulo++;

					if ( strcmp( nomemodulo, procname ) == 0 )
						retcode = true;

                }

                CloseHandle( hProcess ) ;

             }

             // Regardless of OpenProcess success or failure, we
             // still call the enum func with the ProcID.

             //if(!lpProc( lpdwPIDs[dwIndex], 0, szFileName, lParam))
			 //	 break ;


             // Did we just bump into an NTVDM?
			 // Lista processi a 16 nit 
			 /*
             if( _stricmp( szFileName+(strlen(szFileName)-9), "NTVDM.EXE")==0)
             {

                // Fill in some info for the 16-bit enum proc.
                sInfo.dwPID = lpdwPIDs[dwIndex] ;
                //sInfo.lpProc = lpProc ;
                //sInfo.lParam = lParam ;
                sInfo.bEnd = FALSE ;

                // Enum the 16-bit stuff.
                lpfVDMEnumTaskWOWEx( lpdwPIDs[dwIndex], (TASKENUMPROCEX) Enum16, (LPARAM) &sInfo);

                // Did our main enum func say quit?

                if(sInfo.bEnd)
                   break ;
             }
			 */

          }

          HeapFree( GetProcessHeap(), 0, lpdwPIDs ) ;
          //FreeLibrary( hInstLib2 ) ;

       // If Windows 95:

       }else if( osver.dwPlatformId == VER_PLATFORM_WIN32_WINDOWS )

       {

          hInstLib = LoadLibraryA( "Kernel32.DLL" ) ;
          if( hInstLib == NULL )
             return FALSE ;

          // Get procedure addresses.
          // We are linking to these functions of Kernel32
          // explicitly, because otherwise a module using
          // this code would fail to load under Windows NT,
          // which does not have the Toolhelp32
          // functions in the Kernel 32.

          lpfCreateToolhelp32Snapshot=
             (HANDLE(WINAPI *)(DWORD,DWORD))
             GetProcAddress( hInstLib,
             "CreateToolhelp32Snapshot" ) ;

          lpfProcess32First=
             (BOOL(WINAPI *)(HANDLE,LPPROCESSENTRY32))
             GetProcAddress( hInstLib, "Process32First" ) ;

          lpfProcess32Next=
             (BOOL(WINAPI *)(HANDLE,LPPROCESSENTRY32))
             GetProcAddress( hInstLib, "Process32Next" ) ;

          if( lpfProcess32Next == NULL ||
             lpfProcess32First == NULL ||
             lpfCreateToolhelp32Snapshot == NULL )
          {
             FreeLibrary( hInstLib ) ;
             return FALSE ;
          }

          // Get a handle to a Toolhelp snapshot of the systems
          // processes.
          hSnapShot = lpfCreateToolhelp32Snapshot(
             TH32CS_SNAPPROCESS, 0 ) ;
          if( hSnapShot == INVALID_HANDLE_VALUE )
          {
             FreeLibrary( hInstLib ) ;
             return FALSE ;
          }
		  

          // Get the first process' information.
          procentry.dwSize = sizeof(PROCESSENTRY32) ;
          bFlag = lpfProcess32First( hSnapShot, &procentry ) ;

          // While there are processes, keep looping.
          while( bFlag )
		  {

			// rendo minuscolo il modulo
			_strlwr( procentry.szExeFile );
			// estraggo il filename
			nomemodulo = strrchr( procentry.szExeFile, '\\' );
			if ( nomemodulo ) nomemodulo++;

			if ( strcmp( nomemodulo, procname ) == 0 )
				retcode = true;
			  
			  procentry.dwSize = sizeof(PROCESSENTRY32) ;
			   bFlag = lpfProcess32Next( hSnapShot, &procentry );
          }

       }else
          return FALSE ;

       // Free the library.
       FreeLibrary( hInstLib ) ;

       return (retcode) ;
}

