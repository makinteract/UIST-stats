import { parse } from 'jsr:@std/csv';

type Submission = {
  ID: number;
  Subcommittee: string;
  Decision: string;
  ReviewsTotal: number;
  ReviewsDone: number;
  ReviewsLeft: number;
  ReviewsTentative: number;
  Externals: number;
  OverallScore: number;
  OverallStdDev: number;
  Pname: string;
  Pscore: number;
  S1name: string;
  S1score: number;
  E1score: number;
  E2score: number;
};

async function getData(filename: string) {
  const text = await Deno.readTextFile(filename);

  const data = parse(text, {
    skipFirstRow: true,
    strip: true,
  });
  return data.map((row) => ({
    ID: row.ID,
    Subcommittee: row.Subcommittee,
    Decision: row.Decision || '',
    ReviewsTotal: parseInt(row.ReviewsTotal),
    ReviewsDone: parseInt(row.ReviewsDone),
    ReviewsLeft: parseInt(row.ReviewsLeft),
    ReviewsTentative: parseInt(row.ReviewsTentative),
    OverallScore: parseFloat(row.OverallScore),
    OverallStdDev: parseFloat(row.OverallStdDev),
    Pname: row.Pname,
    Pscore: parseFloat(row.Pscore),
    S1name: row.S1name,
    S1score: parseFloat(row.S1score),
    E1score: parseFloat(row.E1score),
    E2score: parseFloat(row.E2score),
  }));
}

const isNan = (value: any) => {
  return value !== value;
};

const raw = await getData('Submissions.csv');
const data = raw.filter(({ Decision }) => Decision === '');
//   .map(({ S1score }) => S1score)
//   .filter((score) => !isNan(score));

// type Occurrences = {
//   [key: number]: number;
// };
// const occurrences = scores.reduce(function (acc: Occurrences, curr) {
//   return acc[curr] ? ++acc[curr] : (acc[curr] = 1), acc;
// }, {});

// console.log(occurrences);
const noReview = data.filter(
  ({ ReviewsDone, ReviewsLeft }) => ReviewsDone === 0 && ReviewsLeft > 0
);

const missingAnExternals = data.filter(
  ({ E1score, E2score }) => isNan(E1score) || isNan(E2score)
);

const missingBothExternals = data.filter(
  ({ E1score, E2score }) => isNan(E1score) && isNan(E2score)
);

const missin2AC = data.filter(({ S1score }) => isNan(S1score));

console.log(raw.length, 'submissions total');
console.log(data.length, 'filtered total');
console.log(raw.length - data.length, 'filtered out');
console.log(noReview.length, 'submissions with no reviews done');
console.log(
  '\t',
  noReview.map(({ ID }) => ID).join(', '),
  'IDs of no reviews done'
);
console.log(
  missingBothExternals.length,
  'submissions with missing both externals'
);
console.log(missingAnExternals.length, 'submissions with missing an external');
console.log(missin2AC.length, 'submissions with missing 2AC reviews');
