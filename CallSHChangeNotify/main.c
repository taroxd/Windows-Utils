
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <stdbool.h>
#include <Shlobj_core.h>

void display_help() {        
    puts("Usage: CallSHChangeNotify OPTION\n"
         "OPTION may only be SHCNE_ASSOCCHANGED currently.");
}

int main(int argc, char *argv[]) {

    if (argc <= 1) {
        display_help();
        return EXIT_FAILURE;
    }
 
    const char *option = argv[1];

    switch (option[0]) {
    case '-': case '/':
        option = option + 1;
        break;
    }

    if (strcmp(option, "?") == 0 || strcmp(option, "help") == 0 || strcmp(option, "h") == 0) {
        display_help();
        return EXIT_FAILURE;
    }

    if (strcmp(argv[1], "SHCNE_ASSOCCHANGED") == 0) {
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, NULL, NULL);
    } else {
        fprintf(stderr, "option not recognized: %s", argv[1]);
        return EXIT_FAILURE;
    }
    
    return EXIT_SUCCESS;
}