//provides a console and a debugprintf function to output to this console
//the console is automatically allocated on the first printf
//note that exitting the console will send a WM_QUIT to the app!!!
//there is also a debug_push and debug_pop function that controls the indentation in debugprintf

#ifndef W2EDEBUG_INCLUDED
#define W2EDEBUG_INCLUDED

#define _CRT_SECURE_NO_DEPRECATE

#include "ALLOCATION.h"

void printindent();
void debugpush();
void debugpop();
void debugprintf(const char * format,...);

#endif