/* 
   
   PCREWrapper.cpp

   Copyright (C) 2004 René Nyffenegger

   This source code is provided 'as-is', without any express or implied
   warranty. In no event will the author be held liable for any damages
   arising from the use of this software.

   Permission is granted to anyone to use this software for any purpose,
   including commercial applications, and to alter it and redistribute it
   freely, subject to the following restrictions:

   1. The origin of this source code must not be misrepresented; you must not
      claim that you wrote the original source code. If you use this source code
      in a product, an acknowledgment in the product documentation would be
      appreciated but is not required.

   2. Altered source versions must be plainly marked as such, and must not be
      misrepresented as being the original source code.

   3. This notice may not be removed or altered from any source distribution.

   René Nyffenegger rene.nyffenegger@adp-gmbh.ch

*/

#include "PCREWrapper.h"

PCREWrapper::PCREWrapper (const std::string& pattern, const std::string& id) {
    const char *error;
    int   error_offset;

   pcre_ = pcre_compile (
          pattern.c_str(),
          0,
         &error,
         &error_offset,
          0);

   if (!pcre_) {
     throw "Error compiling pattern";
   }

  int rc;
  rc = pcre_fullinfo(
    pcre_,                       /* result of pcre_compile() */
    0,                        /* result of pcre_study(), or NULL */
    PCRE_INFO_CAPTURECOUNT,   /* what is required */
    &capture_count_);         /* where to put the data */

  ovector_ = new int[3*(capture_count_+1)];
}

bool PCREWrapper::Match(std::string const& s, std::vector<std::string> &v) {
      int rc = pcre_exec (
         pcre_,                /* the compiled pattern                    */
         0,                    /* no extra data - pattern was not studied */
         s.c_str(),            /* the string to match                     */
         s.length(),           /* the length of the string                */
         0,                    /* start at offset 0 in the subject        */
         0,                    /* default options                         */
         ovector_,             /* output vector for substring information */
       3*(capture_count_+1));  /* number of elements in the output vector */

      if (rc < 0) {
        switch (rc) {
          case PCRE_ERROR_NOMATCH:
            return false;

          default:
            throw "PCRE_ERROR default";
            //::MessageBox(0, "PCRE_ERROR default", 0, 0);
        }
      }
      else {
        for (int i=0; i<rc; i++) {
          v.push_back(s.substr(ovector_[2*i], ovector_[2*i+1] - ovector_[2*i]));
        }
      }

  return true;
}