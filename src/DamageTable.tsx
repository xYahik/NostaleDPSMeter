import { useState, useEffect } from "react";
import { ScrollArea } from "@/components/ui/scroll-area";
import ProgressBar from "./ProgressBar";
import { forwardRef, useImperativeHandle } from "react";

interface Player {
  nick: string;
  dmg: number;
  crit: number;
  hits: number;
  startTime: number;
}

const DamageTable = forwardRef(
  (_props: {}, ref) => {
    useImperativeHandle(ref, () => {
      return {
        UpdatePlayerTable: UpdatePlayer,
        ResetPlayerTable: ResetPlayers,
      };
    });

    const [players, setPlayers] = useState<Player[]>([]);

    const [_totalDMG, setTotalDMG] = useState<number>(
      players.reduce((acc, player) => acc + player.dmg, 0)
    );
    const [bestDmg, setBestDmg] = useState<number>(
      Math.max(...players.map((player) => player.dmg))
    );

    const UpdatePlayer = (event: any) => {

      const parsedPayload = JSON.parse(event.payload);

      setPlayers((prevPlayers) => {
        const existingPlayerIndex = prevPlayers.findIndex(
          (player) => player.nick === parsedPayload.name
        );

        let updatedPlayers;

        if (existingPlayerIndex !== -1) {
          const existingPlayer = prevPlayers[existingPlayerIndex];

          if (
            parsedPayload.dmg !== existingPlayer.dmg ||
            parsedPayload.hits !== existingPlayer.hits ||
            parsedPayload.critHits !== existingPlayer.crit
          ) {
            updatedPlayers = [...prevPlayers];
            updatedPlayers[existingPlayerIndex] = {
              ...existingPlayer,
              dmg: parsedPayload.dmg,
              crit: parsedPayload.critHits,
              hits: parsedPayload.hits,
            };
          } else {
            return prevPlayers;
          }
        } else {
          updatedPlayers = [
            ...prevPlayers,
            {
              nick: parsedPayload.name,
              dmg: parsedPayload.dmg,
              crit: parsedPayload.critHits,
              hits: parsedPayload.hits,
              startTime: Date.now(),
            },
          ];
        }

        return [...updatedPlayers].sort(
          (a, b) => calculateDPS(b) - calculateDPS(a)
        );
      });
    };
    const ResetPlayers = () => {
      setPlayers([]);
    };
    useEffect(() => {

      const interval = setInterval(() => {
        setPlayers((prevPlayers) => {
          const updatedPlayers = prevPlayers.map((player, _index) => ({
            ...player,
          }));

          const newTotalDMG = updatedPlayers.reduce(
            (acc, player) => acc + player.dmg,
            0
          );
          setTotalDMG(newTotalDMG);

          // Recalculate the best DPS
          const currentTime = Date.now();
          const updatedBestDps = Math.max(
            ...updatedPlayers.map(
              (player) => player.dmg / ((currentTime - player.startTime) / 1000)
            ) 
          );
          setBestDmg(updatedBestDps);

          return updatedPlayers;
        });
      }, 100);

      return () => {
        clearInterval(interval);
      };
    }, []);

    const calculateDPS = (player: Player) => {
      const elapsedTimeInSeconds = (Date.now() - player.startTime) / 1000; // Time in seconds
      return player.dmg / elapsedTimeInSeconds;
    };

    return (
      <div className="table-container">
        <h2 className="table-title">Damage Table</h2>

        <div className="table-header">
          <span className="nick">Nick</span>
          <span className="dmg">DMG</span>
          <span className="crit">Crit%</span>
          <span className="hits">Hits</span>
        </div>

        <ScrollArea className="h-[180px] w-[100%] rounded-md border p-4">
          <div className="table-body">
            {players.map((player, index) => {
              const currentTime = Date.now();
              const playerDps =
                player.dmg / ((currentTime - player.startTime) / 1000);
              const CritRate = (player.crit / player.hits) * 100;
              const _progress = bestDmg > 0 ? playerDps / bestDmg : 0;

              return (
                <div key={index} className="table-row">
                  <span className="nick">{player.nick}</span>
                  <span className="dmg">
                    <ProgressBar
                      _progress={_progress * 100}
                      dmg={player.dmg}
                      dmgpersec={Math.round(playerDps)}
                    />
                  </span>
                  <span className="crit">{Math.round(CritRate)}%</span>
                  <span className="hits">{player.hits}</span>
                </div>
              );
            })}
          </div>
        </ScrollArea>

      </div>
    );
  }
);

export default DamageTable;
