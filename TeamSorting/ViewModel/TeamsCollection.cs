using TeamSorting.Model;

namespace TeamSorting.ViewModel;

public class TeamsCollection
{
    public List<Team> Teams = [];

    private Dictionary<DisciplineInfo, double> MaxScores
    {
        get
        {
            var scores = Teams.SelectMany(team => team.Score)
                .GroupBy(scores => scores.Key)
                .Select(groups => groups.MaxBy(pair => pair.Value));
            return new Dictionary<DisciplineInfo, double>(scores);
        }
    }

    private Dictionary<DisciplineInfo, double> MinScores
    {
        get
        {
            var scores = Teams.SelectMany(team => team.Score)
                .GroupBy(scores => scores.Key)
                .Select(groups => groups.MinBy(pair => pair.Value));
            return new Dictionary<DisciplineInfo, double>(scores);
        }
    }

    public Dictionary<Team, Dictionary<DisciplineInfo, double>> NormalizedDisciplines
    {
        get
        {
            var dict = new Dictionary<Team, Dictionary<DisciplineInfo, double>>();
            var maxScores = MaxScores;
            var minScores = MinScores;

            var disciplines = maxScores.Select(pair => pair.Key).ToList();

            foreach (var team in Teams)
            {
                var score = team.Score;
                var disciplineDict = new Dictionary<DisciplineInfo, double>();
                foreach (var discipline in disciplines)
                {
                    var normalizedValue = NormalizeValue(
                        value: score.First(pair => pair.Key == discipline).Value,
                        minValue: minScores.First(pair => pair.Key == discipline).Value,
                        maxValue: maxScores.First(pair => pair.Key == discipline).Value,
                        min: 0,
                        max: 100);
                    disciplineDict.Add(discipline, normalizedValue);
                }

                dict.Add(team, disciplineDict);
            }

            return dict;
        }
    }

    private static double NormalizeValue(double value, double minValue, double maxValue, double min, double max)
    {
        return (((value - minValue) / (maxValue - minValue)) * (max - min)) + min;
    }
}