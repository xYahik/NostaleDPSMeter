"use client";

import { Progress } from "@/components/ui/progress";

type ProgressProps = {
  _progress: number;
  dmg: number;
  dmgpersec: number;
};


export default function ProgressGradient({ _progress, dmg, dmgpersec }: ProgressProps) {
  const progress = _progress;

  return (
  <div className="">
    <Progress
      value={progress}
      className="translate-y-3 h-5 w-[100%] [&>div]:bg-gradient-to-r [&>div]:from-red-400 [&>div]:via-yellow-500 [&>div]:to-green-500 [&>div]:rounded-l-full"
    />
   <div className="flex justify-between px-1.5 -translate-y-3 text-sm font-medium text-primary-foreground"       style={{
		fontSize: '12px',
       textShadow: `
         -1px -1px 0 black,  
          1px -1px 0 black,  
         -1px  1px 0 black,  
          1px  1px 0 black
       `
     }}>
  <span>{dmg}</span>
  <span>{dmgpersec}/s</span>
   </div>
   </div>
  );
}
